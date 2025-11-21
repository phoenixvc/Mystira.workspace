using Mystira.StoryGenerator.RagIndexer.Models;

namespace Mystira.StoryGenerator.RagIndexer.Interfaces;

public interface IAzureAISearchService
{
    Task EnsureIndexExistsAsync();
    Task IndexChunkAsync(InstructionChunk chunk, string dataset, string version, IReadOnlyList<float> embedding);
    Task DeleteDatasetAsync(string dataset, string version);
}