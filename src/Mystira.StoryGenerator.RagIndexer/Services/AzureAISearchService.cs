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
    private readonly SearchClient _searchClient;
    private readonly string _indexName;
    private readonly ILoggerService _logger;
    private readonly IRetryPolicyService _retryPolicy;

    public AzureAISearchService(
        AzureAISearchSettings settings, 
        ILoggerService logger,
        IRetryPolicyService retryPolicy)
    {
        _indexName = settings.IndexName;
        _logger = logger;
        _retryPolicy = retryPolicy;
        
        var credentials = new AzureKeyCredential(settings.ApiKey);
        _indexClient = new SearchIndexClient(new Uri(settings.Endpoint), credentials);
        _searchClient = new SearchClient(new Uri(settings.Endpoint), _indexName, credentials);
    }

    public async Task EnsureIndexExistsAsync()
    {
        await _retryPolicy.ExecuteWithRetryAsync(async () =>
        {
            try
            {
                await _indexClient.GetIndexAsync(_indexName);
                _logger.LogInfo($"Index '{_indexName}' already exists.");
                return true;
            }
            catch (RequestFailedException ex) when (ex.Status == 404)
            {
                _logger.LogInfo($"Creating index '{_indexName}'...");
                
                var index = CreateSearchIndexDefinition();
                await _indexClient.CreateIndexAsync(index);
                _logger.LogInfo($"Index '{_indexName}' created successfully.");
                return true;
            }
        }, "EnsureIndexExistsAsync");
    }

    private SearchIndex CreateSearchIndexDefinition()
    {
        return new SearchIndex(_indexName)
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

                // Legacy fields for backward compatibility
                new SimpleField("chunk_id", SearchFieldDataType.String) { IsKey = true },
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

    public async Task IndexChunkAsync(InstructionChunk chunk, string dataset, string version, IReadOnlyList<float> embedding)
    {
        await _retryPolicy.ExecuteWithRetryAsync(async () =>
        {
            var document = CreateSearchDocument(chunk, dataset, version, embedding);
            var batch = CreateIndexBatch(document);

            var response = await _searchClient.IndexDocumentsAsync(batch);
            _logger.LogInfo($"Indexed chunk '{chunk.ChunkId}': {response.Value.Results.First().Succeeded}");
            
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
            ["version"] = chunk.Version,
            ["createdAt"] = chunk.CreatedAt,
            ["updatedAt"] = chunk.UpdatedAt,
            
            // Legacy fields for backward compatibility
            ["chunk_id"] = chunk.ChunkId,
            ["section"] = chunk.Section,
            ["dataset"] = dataset,
            ["version"] = version,
            ["keywords"] = chunk.Keywords,
            
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

    public async Task DeleteDatasetAsync(string dataset, string version)
    {
        await _retryPolicy.ExecuteWithRetryAsync(async () =>
        {
            var filter = $"dataset eq '{dataset}' and version eq '{version}'";
            var response = await _searchClient.DeleteDocumentsAsync(filter);
            _logger.LogInfo($"Deleted {response.Value.Results.Count} documents from dataset '{dataset}' version '{version}'");
            
            return response;
        }, $"DeleteDatasetAsync({dataset}, {version})");
    }
}