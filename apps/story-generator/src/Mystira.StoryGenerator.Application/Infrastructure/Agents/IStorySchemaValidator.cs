namespace Mystira.StoryGenerator.Application.Infrastructure.Agents;

/// <summary>
/// Validates story JSON against the Foundry story schema.
/// </summary>
public interface IStorySchemaValidator
{
    /// <summary>
    /// Validates the given story JSON string against the schema.
    /// </summary>
    /// <param name="storyJson">The story JSON to validate.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A tuple indicating whether the JSON is valid and a list of validation errors.</returns>
    Task<(bool IsValid, List<string> Errors)> ValidateAsync(string storyJson, CancellationToken cancellationToken = default);
}
