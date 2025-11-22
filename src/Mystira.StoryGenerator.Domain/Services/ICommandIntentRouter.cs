namespace Mystira.StoryGenerator.Llm.Services.Intent;

public interface ICommandIntentRouter
{
    Task<object?> RouteIntentToCommandAsync(string userQuery, object? context = null, CancellationToken cancellationToken = default);
    Task<string?> DetectPrimaryInstructionTypeAsync(string userQuery, CancellationToken cancellationToken = default);
}
