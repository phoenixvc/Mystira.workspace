using Mystira.StoryGenerator.RagIndexer.Interfaces;
using Mystira.StoryGenerator.RagIndexer.Models;

namespace Mystira.StoryGenerator.RagIndexer.Services;

public class RagIndexingService : IRagIndexingService
{
    private readonly IAzureAISearchService _searchService;
    private readonly IAzureOpenAIEmbeddingService _embeddingService;
    private readonly ILoggerService _logger;

    public RagIndexingService(
        IAzureAISearchService searchService, 
        IAzureOpenAIEmbeddingService embeddingService,
        ILoggerService logger)
    {
        _searchService = searchService;
        _embeddingService = embeddingService;
        _logger = logger;
    }

    public async Task IndexDatasetAsync(RagIndexRequest request)
    {
        _logger.LogInfo($"Starting indexing for dataset: {request.Dataset} v{request.Version}");
        _logger.LogInfo($"Found {request.Chunks.Count} chunks to process");

        // Ensure index exists
        await _searchService.EnsureIndexExistsAsync();

        // Delete existing documents for this dataset/version combination
        _logger.LogInfo($"Removing existing documents for dataset '{request.Dataset}' version '{request.Version}'");
        await _searchService.DeleteDatasetAsync(request.Dataset, request.Version);

        // Process each chunk
        var results = await ProcessChunksAsync(request.Chunks, request.Dataset, request.Version);
        
        LogResults(results);
    }

    private async Task<(int successCount, int errorCount)> ProcessChunksAsync(
        List<InstructionChunk> chunks, string dataset, string version)
    {
        int successCount = 0;
        int errorCount = 0;

        foreach (var chunk in chunks)
        {
            var result = await ProcessChunkAsync(chunk, dataset, version);
            if (result)
            {
                successCount++;
            }
            else
            {
                errorCount++;
            }
        }

        return (successCount, errorCount);
    }

    private async Task<bool> ProcessChunkAsync(InstructionChunk chunk, string dataset, string version)
    {
        try
        {
            _logger.LogInfo($"Processing chunk: {chunk.ChunkId} - {chunk.Title}");
            
            // Generate embedding for content
            var embedding = await _embeddingService.GenerateEmbeddingAsync(chunk.Content);

            // Index the chunk with its embedding
            await _searchService.IndexChunkAsync(chunk, dataset, version, embedding);
            
            _logger.LogInfo($"Successfully indexed chunk: {chunk.ChunkId}");
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError($"Failed to process chunk {chunk.ChunkId}: {ex.Message}", ex);
            return false;
        }
    }

    private void LogResults((int successCount, int errorCount) results)
    {
        _logger.LogInfo($"Indexing completed. Success: {results.successCount}, Errors: {results.errorCount}");
        
        if (results.errorCount > 0)
        {
            _logger.LogWarning($"Some chunks failed to process. Check logs for details.");
        }
    }
}