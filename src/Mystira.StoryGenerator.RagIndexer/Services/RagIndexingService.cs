using Mystira.StoryGenerator.RagIndexer.Configuration;
using Mystira.StoryGenerator.RagIndexer.Models;

namespace Mystira.StoryGenerator.RagIndexer.Services;

public class RagIndexingService
{
    private readonly AzureAISearchService _searchService;
    private readonly AzureOpenAIEmbeddingService _embeddingService;

    public RagIndexingService(AzureAISearchService searchService, AzureOpenAIEmbeddingService embeddingService)
    {
        _searchService = searchService;
        _embeddingService = embeddingService;
    }

    public async Task IndexDatasetAsync(RagIndexRequest request)
    {
        Console.WriteLine($"Starting indexing for dataset: {request.Dataset} v{request.Version}");
        Console.WriteLine($"Found {request.Chunks.Count} chunks to process");

        // Ensure the index exists
        await _searchService.EnsureIndexExistsAsync();

        // Delete existing documents for this dataset/version combination
        Console.WriteLine($"Removing existing documents for dataset '{request.Dataset}' version '{request.Version}'");
        await _searchService.DeleteDatasetAsync(request.Dataset, request.Version);

        // Process each chunk
        var successCount = 0;
        var errorCount = 0;

        foreach (var chunk in request.Chunks)
        {
            try
            {
                Console.WriteLine($"Processing chunk: {chunk.ChunkId} - {chunk.Title}");
                
                // Generate embedding for the content
                var embedding = await _embeddingService.GenerateEmbeddingAsync(chunk.Content);
                Console.WriteLine($"Generated embedding with {embedding.Count} dimensions");

                // Index the chunk with its embedding
                await _searchService.IndexChunkAsync(chunk, request.Dataset, request.Version, embedding);
                
                successCount++;
                Console.WriteLine($"Successfully indexed chunk: {chunk.ChunkId}");
            }
            catch (Exception ex)
            {
                errorCount++;
                Console.WriteLine($"Failed to process chunk {chunk.ChunkId}: {ex.Message}");
            }
        }

        Console.WriteLine($"Indexing completed. Success: {successCount}, Errors: {errorCount}");
    }
}