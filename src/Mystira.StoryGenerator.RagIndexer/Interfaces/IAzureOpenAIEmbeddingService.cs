namespace Mystira.StoryGenerator.RagIndexer.Interfaces;

public interface IAzureOpenAIEmbeddingService
{
    Task<IReadOnlyList<float>> GenerateEmbeddingAsync(string text);
}