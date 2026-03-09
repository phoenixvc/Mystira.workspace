using Mystira.StoryGenerator.Domain.Stories;

namespace Mystira.StoryGenerator.Domain.Services;

/// <summary>
/// Supported content formats for scenario creation.
/// </summary>
public enum ScenarioContentFormat
{
    Json,
    Yaml
}

/// <summary>
/// Factory for creating <see cref="Scenario"/> instances from textual content.
/// </summary>
public interface IScenarioFactory
{
    /// <summary>
    /// Creates a <see cref="Scenario"/> from the provided content using the specified format.
    /// </summary>
    /// <param name="content">The raw scenario content (JSON or YAML).</param>
    /// <param name="format">The format of <paramref name="content"/>.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>The deserialized <see cref="Scenario"/>.</returns>
    Task<Scenario> CreateFromContentAsync(string content, ScenarioContentFormat format, CancellationToken cancellationToken = default);
}
