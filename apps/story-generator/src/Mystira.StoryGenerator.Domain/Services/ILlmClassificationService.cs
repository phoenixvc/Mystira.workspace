namespace Mystira.StoryGenerator.Domain.Services;

public interface ILlmClassificationService<T>
{
    Task<T?> ClassifyAsync(string sceneContent, CancellationToken cancellationToken = default);
}
