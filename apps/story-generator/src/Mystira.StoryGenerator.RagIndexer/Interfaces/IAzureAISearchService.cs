using Mystira.StoryGenerator.RagIndexer.Models;

namespace Mystira.StoryGenerator.RagIndexer.Interfaces;

public interface IAzureAISearchService
{
    Task EnsureIndexExistsAsync(string? ageGroup = null);
    Task IndexChunkAsync(InstructionChunk chunk, string dataset, string version, IReadOnlyList<float> embedding, string? ageGroup = null);
    Task DeleteDatasetAsync(string dataset, string version, string? ageGroup = null);
}