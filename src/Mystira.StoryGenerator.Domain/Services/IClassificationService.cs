namespace Mystira.StoryGenerator.Domain.Services;

public interface IClassificationService<T>
{
    Task<T?> ClassifyAsync(string sceneContent, CancellationToken cancellationToken = default);
}
