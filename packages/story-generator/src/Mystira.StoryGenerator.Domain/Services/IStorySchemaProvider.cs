namespace Mystira.StoryGenerator.Domain.Services;

public interface IStorySchemaProvider
{
    Task<string?> GetSchemaJsonAsync(CancellationToken cancellationToken = default);
    bool IsStrict { get; }
}
