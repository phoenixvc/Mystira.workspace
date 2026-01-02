using Mystira.Authoring.Abstractions.Commands;
using Mystira.Authoring.Abstractions.Services;

namespace Mystira.Authoring.Commands.Stories;

/// <summary>
/// Command to validate a story against the schema.
/// </summary>
public class ValidateStoryCommand : ICommand<StoryValidationResult>
{
    /// <summary>
    /// JSON content to validate.
    /// </summary>
    public string JsonContent { get; set; } = string.Empty;
}
