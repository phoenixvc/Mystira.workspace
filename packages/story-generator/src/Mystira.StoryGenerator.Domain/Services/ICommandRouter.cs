namespace Mystira.StoryGenerator.Domain.Services;

public interface ICommandRouter
{
    Task<string?> DetectPrimaryInstructionTypeAsync(string userQuery, CancellationToken cancellationToken = default);
}
