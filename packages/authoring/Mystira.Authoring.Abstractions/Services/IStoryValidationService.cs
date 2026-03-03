using Mystira.Authoring.Abstractions.Models.Scenario;

namespace Mystira.Authoring.Abstractions.Services;

/// <summary>
/// Service for validating story scenarios.
/// </summary>
public interface IStoryValidationService
{
    /// <summary>
    /// Validates a scenario against the story schema.
    /// </summary>
    /// <param name="scenario">The scenario to validate.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Validation result with any issues found.</returns>
    Task<StoryValidationResult> ValidateAsync(Scenario scenario, CancellationToken cancellationToken = default);

    /// <summary>
    /// Validates JSON story content against the schema.
    /// </summary>
    /// <param name="jsonContent">The JSON content to validate.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Validation result with any issues found.</returns>
    Task<StoryValidationResult> ValidateJsonAsync(string jsonContent, CancellationToken cancellationToken = default);
}

/// <summary>
/// Result of story validation.
/// </summary>
public class StoryValidationResult
{
    /// <summary>
    /// Whether the validation passed.
    /// </summary>
    public bool IsValid { get; set; }

    /// <summary>
    /// List of validation errors.
    /// </summary>
    public List<ValidationError> Errors { get; set; } = new();

    /// <summary>
    /// List of validation warnings.
    /// </summary>
    public List<ValidationWarning> Warnings { get; set; } = new();
}

/// <summary>
/// A validation error.
/// </summary>
public class ValidationError
{
    /// <summary>
    /// Path to the element with the error.
    /// </summary>
    public string Path { get; set; } = string.Empty;

    /// <summary>
    /// Error message.
    /// </summary>
    public string Message { get; set; } = string.Empty;

    /// <summary>
    /// Error code.
    /// </summary>
    public string? Code { get; set; }
}

/// <summary>
/// A validation warning.
/// </summary>
public class ValidationWarning
{
    /// <summary>
    /// Path to the element with the warning.
    /// </summary>
    public string Path { get; set; } = string.Empty;

    /// <summary>
    /// Warning message.
    /// </summary>
    public string Message { get; set; } = string.Empty;

    /// <summary>
    /// Warning code.
    /// </summary>
    public string? Code { get; set; }
}
