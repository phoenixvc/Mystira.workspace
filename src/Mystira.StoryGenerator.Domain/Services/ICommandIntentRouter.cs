namespace Mystira.StoryGenerator.Domain.Services;

public interface ICommandIntentRouter
{
    Task<string?> DetectPrimaryInstructionTypeAsync(string userQuery, CancellationToken cancellationToken = default);
}
