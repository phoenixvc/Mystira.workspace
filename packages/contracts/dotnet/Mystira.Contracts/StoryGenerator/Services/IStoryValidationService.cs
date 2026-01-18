using Mystira.Contracts.StoryGenerator.Stories;

namespace Mystira.Contracts.StoryGenerator.Services;

/// <summary>
/// Service for validating story scenarios.
/// </summary>
public interface IStoryValidationService
{
    /// <summary>
    /// Validates a scenario object.
    /// </summary>
    /// <param name="scenario">The scenario to validate.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The validation result.</returns>
    Task<StoryValidationResult> ValidateAsync(
        Scenario scenario,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Validates a story JSON string.
    /// </summary>
    /// <param name="json">The JSON to validate.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The validation result.</returns>
    Task<StoryValidationResult> ValidateJsonAsync(
        string json,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Validates a story YAML string.
    /// </summary>
    /// <param name="yaml">The YAML to validate.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The validation result.</returns>
    Task<StoryValidationResult> ValidateYamlAsync(
        string yaml,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Performs deep validation including narrative consistency checks.
    /// </summary>
    /// <param name="scenario">The scenario to validate.</param>
    /// <param name="options">Validation options.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The validation result.</returns>
    Task<StoryValidationResult> DeepValidateAsync(
        Scenario scenario,
        ValidationOptions? options = null,
        CancellationToken cancellationToken = default);
}

/// <summary>
/// Options for story validation.
/// </summary>
public class ValidationOptions
{
    /// <summary>
    /// Whether to validate schema conformance.
    /// </summary>
    public bool ValidateSchema { get; set; } = true;

    /// <summary>
    /// Whether to validate graph structure (connectivity, reachability).
    /// </summary>
    public bool ValidateStructure { get; set; } = true;

    /// <summary>
    /// Whether to validate narrative consistency.
    /// </summary>
    public bool ValidateConsistency { get; set; } = true;

    /// <summary>
    /// Whether to validate entity continuity.
    /// </summary>
    public bool ValidateEntities { get; set; } = true;

    /// <summary>
    /// Minimum validation score to pass.
    /// </summary>
    public double MinScore { get; set; } = 0.7;

    /// <summary>
    /// Whether to include suggestions in the result.
    /// </summary>
    public bool IncludeSuggestions { get; set; } = true;
}
