using Mystira.Authoring.Abstractions.Models.Scenario;

namespace Mystira.Authoring.Abstractions.Services;

/// <summary>
/// Supported content formats for scenario creation.
/// </summary>
public enum ScenarioContentFormat
{
    /// <summary>
    /// JSON format.
    /// </summary>
    Json,

    /// <summary>
    /// YAML format.
    /// </summary>
    Yaml
}

/// <summary>
/// Factory for creating Scenario instances from textual content.
/// </summary>
public interface IScenarioFactory
{
    /// <summary>
    /// Creates a Scenario from the provided content using the specified format.
    /// </summary>
    /// <param name="content">The raw scenario content (JSON or YAML).</param>
    /// <param name="format">The format of the content.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>The deserialized Scenario.</returns>
    Task<Scenario> CreateFromContentAsync(
        string content,
        ScenarioContentFormat format,
        CancellationToken cancellationToken = default);
}
