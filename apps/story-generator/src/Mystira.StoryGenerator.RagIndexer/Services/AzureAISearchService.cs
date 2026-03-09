using Azure;
using Azure.Search.Documents;
using Azure.Search.Documents.Indexes;
using Azure.Search.Documents.Indexes.Models;
using Azure.Search.Documents.Models;
using Mystira.StoryGenerator.RagIndexer.Configuration;
using Mystira.StoryGenerator.RagIndexer.Interfaces;
using Mystira.StoryGenerator.RagIndexer.Models;

namespace Mystira.StoryGenerator.RagIndexer.Services;

public class AzureAISearchService : IAzureAISearchService
{
    private readonly SearchIndexClient _indexClient;
    private readonly Dictionary<string, SearchClient> _searchClientsByIndex;
    private readonly string _defaultIndexName;
    private readonly AzureAISearchSettings _settings;
    private readonly ILoggerService _logger;
    private readonly IRetryPolicyService _retryPolicy;

    public AzureAISearchService(
        AzureAISearchSettings settings,
        ILoggerService logger,
        IRetryPolicyService retryPolicy)
    {
        _defaultIndexName = settings.IndexName;
        _settings = settings;
        _logger = logger;
        _retryPolicy = retryPolicy;
        _searchClientsByIndex = new Dictionary<string, SearchClient>();

        var credentials = new AzureKeyCredential(settings.ApiKey);
        _indexClient = new SearchIndexClient(new Uri(settings.Endpoint), credentials);
        
        // Initialize search clients for all configured indexes
        var indexNames = new HashSet<string> { _defaultIndexName };
        foreach (var indexName in settings.AgeGroupIndexMapping.Values)
        {
            if (!string.IsNullOrWhiteSpace(indexName))
            {
                indexNames.Add(indexName);
            }
        }

        foreach (var indexName in indexNames)
        {
            _searchClientsByIndex[indexName] = new SearchClient(new Uri(settings.Endpoint), indexName, credentials);
        }
    }

    private string ResolveIndexName(string? ageGroup)
    {
        if (string.IsNullOrWhiteSpace(ageGroup))
        {
            return _defaultIndexName;
        }

        if (_settings.AgeGroupIndexMapping.TryGetValue(ageGroup, out var indexName))
        {
            return indexName;
        }

        return _defaultIndexName;
    }

    private SearchClient GetSearchClient(string indexName)
    {
        if (_searchClientsByIndex.TryGetValue(indexName, out var client))
        {
            return client;
        }

        // Fallback to default if not found
        return _searchClientsByIndex[_defaultIndexName];
    }

    public async Task EnsureIndexExistsAsync(string? ageGroup = null)
    {
        var indexName = ResolveIndexName(ageGroup);
        await _retryPolicy.ExecuteWithRetryAsync(async () =>
        {
            try
            {
                await _indexClient.GetIndexAsync(indexName);
                _logger.LogInfo($"Index '{indexName}' already exists.");
                return true;
            }
            catch (RequestFailedException ex) when (ex.Status == 404)
            {
                _logger.LogInfo($"Creating index '{indexName}'...");

                var index = CreateSearchIndexDefinition(indexName);
                await _indexClient.CreateIndexAsync(index);
                _logger.LogInfo($"Index '{indexName}' created successfully.");
                return true;
            }
        }, "EnsureIndexExistsAsync");
    }

    private SearchIndex CreateSearchIndexDefinition(string indexName)
    {
        return new SearchIndex(indexName)
        {
            Fields = new List<SearchField>()
            {
                // Primary Key
                new SimpleField("id", SearchFieldDataType.String) { IsKey = true },

                // Content
                new SearchField("content", SearchFieldDataType.String)
                {
                    IsSearchable = true,
                    IsFilterable = false,
                    AnalyzerName = "standard.lucene"
                },

                new SearchField("title", SearchFieldDataType.String)
                {
                    IsSearchable = true,
                    IsFilterable = true
                },

                // Instruction categorization
                new SimpleField("category", SearchFieldDataType.String)
                {
                    IsFilterable = true,
                    IsFacetable = true
                },

                new SimpleField("subcategory", SearchFieldDataType.String)
                {
                    IsFilterable = true,
                    IsFacetable = true
                },

                // Instruction type (missing before)
                new SimpleField("instructionType", SearchFieldDataType.String)
                {
                    IsFilterable = true,
                    IsFacetable = true
                },

                // Priority and importance
                new SimpleField("priority", SearchFieldDataType.String)
                {
                    IsFilterable = true,
                    IsFacetable = true
                },

                new SimpleField("isMandatory", SearchFieldDataType.Boolean)
                {
                    IsFilterable = true
                },

                // Context and relationships
                new SearchField("examples", SearchFieldDataType.String)
                {
                    IsFilterable = false
                },

                new SimpleField("tags", SearchFieldDataType.Collection(SearchFieldDataType.String))
                {
                    IsFilterable = true,
                    IsFacetable = true
                },

                // Metadata
                new SimpleField("source", SearchFieldDataType.String)
                {
                    IsFilterable = true
                },

                new SimpleField("version", SearchFieldDataType.String)
                {
                    IsFilterable = true
                },

                // Timestamps
                new SimpleField("createdAt", SearchFieldDataType.DateTimeOffset)
                {
                    IsFilterable = true,
                    IsSortable = true
                },

                new SimpleField("updatedAt", SearchFieldDataType.DateTimeOffset)
                {
                    IsFilterable = true,
                    IsSortable = true
                },

                // Additional metadata and filterable fields
                new SearchField("section", SearchFieldDataType.String) { IsSearchable = true, IsFilterable = true },
                new SimpleField("dataset", SearchFieldDataType.String) { IsFilterable = true, IsFacetable = true },
                new SearchField("keywords", SearchFieldDataType.Collection(SearchFieldDataType.String)) { IsFilterable = true, IsFacetable = true },

                // Vector field
                new SearchField("embedding", SearchFieldDataType.Collection(SearchFieldDataType.Single))
                {
                    IsSearchable = true,
                    IsFilterable = false,
                    VectorSearchDimensions = 1536, // OpenAI ada-002 dimensions
                    VectorSearchProfileName = "my-vector-config"
                }
            },
            VectorSearch = new VectorSearch
            {
                Algorithms =
                {
                    new HnswAlgorithmConfiguration("my-hnsw-config")
                },
                Profiles =
                {
                    new VectorSearchProfile("my-vector-config", "my-hnsw-config")
                }
            }
        };
    }

    public async Task IndexChunkAsync(InstructionChunk chunk, string dataset, string version, IReadOnlyList<float> embedding, string? ageGroup = null)
    {
        // Guard clauses outside of retry to keep generic result type consistent
        if (embedding == null || embedding.Count == 0)
        {
            _logger.LogWarning($"Embedding missing for chunk '{chunk.ChunkId}'. Skipping index.");
            return;
        }
        if (embedding.Count != 1536)
        {
            _logger.LogWarning($"Unexpected embedding dimensions {embedding.Count} for chunk '{chunk.ChunkId}'. Expected 1536. Proceeding to index.");
        }

        var indexName = ResolveIndexName(ageGroup);
        var searchClient = GetSearchClient(indexName);

        await _retryPolicy.ExecuteWithRetryAsync(async () =>
        {
            var document = CreateSearchDocument(chunk, dataset, version, embedding);
            var batch = CreateIndexBatch(document);

            var response = await searchClient.IndexDocumentsAsync(batch);
            _logger.LogInfo($"Indexed chunk '{chunk.ChunkId}' into '{indexName}': {response.Value.Results.First().Succeeded}");

            return response;
        }, $"IndexChunkAsync({chunk.ChunkId})");
    }

    private SearchDocument CreateSearchDocument(InstructionChunk chunk, string dataset, string version, IReadOnlyList<float> embedding)
    {
        return new SearchDocument
        {
            // Primary Key
            ["id"] = chunk.ChunkId,

            // Content
            ["content"] = chunk.Content,
            ["title"] = chunk.Title,

            // Instruction categorization
            ["category"] = chunk.Category,
            ["subcategory"] = chunk.Subcategory,
            ["instructionType"] = chunk.InstructionType,
            ["priority"] = chunk.Priority,
            ["isMandatory"] = chunk.IsMandatory,
            ["examples"] = chunk.Examples,
            ["tags"] = chunk.Tags,

            // Context and relationships
            ["section"] = chunk.Section,
            ["keywords"] = chunk.Keywords,

            // Metadata
            ["source"] = chunk.Source,
            ["version"] = version,
            ["createdAt"] = chunk.CreatedAt,
            ["updatedAt"] = chunk.UpdatedAt,

            // Dataset scope
            ["dataset"] = dataset,

            // Vector embedding
            ["embedding"] = embedding
        };
    }

    private IndexDocumentsBatch<SearchDocument> CreateIndexBatch(SearchDocument document)
    {
        return new IndexDocumentsBatch<SearchDocument>
        {
            Actions =
            {
                IndexDocumentsAction.Upload(document)
            }
        };
    }

    public async Task DeleteDatasetAsync(string dataset, string version, string? ageGroup = null)
    {
        var indexName = ResolveIndexName(ageGroup);
        var searchClient = GetSearchClient(indexName);

        await _retryPolicy.ExecuteWithRetryAsync(async () =>
        {
            int deleted = 0;
            var options = new SearchOptions
            {
                Filter = $"dataset eq '{dataset}' and version eq '{version}'",
                Size = 1000
            };
            options.Select.Add("id");

            // Execute a search to get documents to delete
            var searchResponse = await searchClient.SearchAsync<SearchDocument>("*", options);

            // Iterate over results pages
            await foreach (var result in searchResponse.Value.GetResultsAsync())
            {
                var key = result.Document["id"]?.ToString();
                if (string.IsNullOrEmpty(key)) continue;

                var batch = new IndexDocumentsBatch<SearchDocument>
                {
                    Actions = { IndexDocumentsAction.Delete(new SearchDocument { ["id"] = key! }) }
                };

                var resp = await searchClient.IndexDocumentsAsync(batch);
                deleted += resp.Value.Results.Count(x => x.Succeeded);
            }

            _logger.LogInfo($"Deleted {deleted} documents from dataset '{dataset}' version '{version}' in index '{indexName}'");
            return deleted;
        }, $"DeleteDatasetAsync({dataset}, {version})");
    }
}
