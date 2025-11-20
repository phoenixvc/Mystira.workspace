using Azure;
using Azure.Search.Documents;
using Azure.Search.Documents.Indexes;
using Azure.Search.Documents.Indexes.Models;
using Azure.Search.Documents.Models;
using Mystira.StoryGenerator.RagIndexer.Configuration;
using Mystira.StoryGenerator.RagIndexer.Models;

namespace Mystira.StoryGenerator.RagIndexer.Services;

public class AzureAISearchService
{
    private readonly SearchIndexClient _indexClient;
    private readonly SearchClient _searchClient;
    private readonly string _indexName;

    public AzureAISearchService(AzureAISearchSettings settings)
    {
        _indexName = settings.IndexName;
        
        var credentials = new AzureKeyCredential(settings.ApiKey);
        _indexClient = new SearchIndexClient(new Uri(settings.Endpoint), credentials);
        _searchClient = new SearchClient(new Uri(settings.Endpoint), _indexName, credentials);
    }

    public async Task EnsureIndexExistsAsync()
    {
        try
        {
            await _indexClient.GetIndexAsync(_indexName);
            Console.WriteLine($"Index '{_indexName}' already exists.");
        }
        catch (RequestFailedException ex) when (ex.Status == 404)
        {
            Console.WriteLine($"Creating index '{_indexName}'...");
            
            var index = new SearchIndex(_indexName)
            {
                Fields = new List<SearchField>()
                {
                    new SimpleField("chunk_id", SearchFieldDataType.String) { IsKey = true },
                    new SearchField("content", SearchFieldDataType.String) { IsSearchable = true, IsFilterable = false },
                    new SearchField("title", SearchFieldDataType.String) { IsSearchable = true, IsFilterable = true },
                    new SearchField("section", SearchFieldDataType.String) { IsSearchable = true, IsFilterable = true },
                    new SearchField("dataset", SearchFieldDataType.String) { IsFilterable = true, IsFacetable = true },
                    new SearchField("version", SearchFieldDataType.String) { IsFilterable = true },
                    new SearchField("keywords", SearchFieldDataType.Collection(SearchFieldDataType.String)) { IsFilterable = true, IsFacetable = true },
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

            await _indexClient.CreateIndexAsync(index);
            Console.WriteLine($"Index '{_indexName}' created successfully.");
        }
    }

    public async Task IndexChunkAsync(InstructionChunk chunk, string dataset, string version, IReadOnlyList<float> embedding)
    {
        var document = new SearchDocument
        {
            ["chunk_id"] = chunk.ChunkId,
            ["content"] = chunk.Content,
            ["title"] = chunk.Title,
            ["section"] = chunk.Section,
            ["dataset"] = dataset,
            ["version"] = version,
            ["keywords"] = chunk.Keywords,
            ["embedding"] = embedding
        };

        var batch = new IndexDocumentsBatch<SearchDocument>
        {
            Actions = 
            {
                IndexDocumentsAction.Upload(document)
            }
        };

        try
        {
            var response = await _searchClient.IndexDocumentsAsync(batch);
            Console.WriteLine($"Indexed chunk '{chunk.ChunkId}': {response.Value.Results.First().Succeeded}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error indexing chunk '{chunk.ChunkId}': {ex.Message}");
            throw;
        }
    }

    public async Task DeleteDatasetAsync(string dataset, string version)
    {
        var filter = $"dataset eq '{dataset}' and version eq '{version}'";
        
        try
        {
            var response = await _searchClient.DeleteDocumentsAsync(filter);
            Console.WriteLine($"Deleted {response.Value.Results.Count} documents from dataset '{dataset}' version '{version}'");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error deleting documents: {ex.Message}");
            throw;
        }
    }
}