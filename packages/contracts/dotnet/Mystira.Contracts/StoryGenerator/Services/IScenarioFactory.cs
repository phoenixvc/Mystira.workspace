using Mystira.Contracts.StoryGenerator.Stories;

namespace Mystira.Contracts.StoryGenerator.Services;

/// <summary>
/// Factory for creating scenario instances from various formats.
/// </summary>
public interface IScenarioFactory
{
    /// <summary>
    /// Creates a scenario from JSON.
    /// </summary>
    /// <param name="json">The JSON string.</param>
    /// <returns>The parsed scenario.</returns>
    Scenario FromJson(string json);

    /// <summary>
    /// Creates a scenario from YAML.
    /// </summary>
    /// <param name="yaml">The YAML string.</param>
    /// <returns>The parsed scenario.</returns>
    Scenario FromYaml(string yaml);

    /// <summary>
    /// Tries to create a scenario from JSON.
    /// </summary>
    /// <param name="json">The JSON string.</param>
    /// <param name="scenario">The parsed scenario if successful.</param>
    /// <param name="error">Error message if parsing failed.</param>
    /// <returns>True if parsing succeeded.</returns>
    bool TryFromJson(string json, out Scenario? scenario, out string? error);

    /// <summary>
    /// Tries to create a scenario from YAML.
    /// </summary>
    /// <param name="yaml">The YAML string.</param>
    /// <param name="scenario">The parsed scenario if successful.</param>
    /// <param name="error">Error message if parsing failed.</param>
    /// <returns>True if parsing succeeded.</returns>
    bool TryFromYaml(string yaml, out Scenario? scenario, out string? error);

    /// <summary>
    /// Serializes a scenario to JSON.
    /// </summary>
    /// <param name="scenario">The scenario to serialize.</param>
    /// <param name="indented">Whether to indent the output.</param>
    /// <returns>The JSON string.</returns>
    string ToJson(Scenario scenario, bool indented = true);

    /// <summary>
    /// Serializes a scenario to YAML.
    /// </summary>
    /// <param name="scenario">The scenario to serialize.</param>
    /// <returns>The YAML string.</returns>
    string ToYaml(Scenario scenario);

    /// <summary>
    /// Creates a new empty scenario with default values.
    /// </summary>
    /// <returns>A new scenario instance.</returns>
    Scenario CreateEmpty();

    /// <summary>
    /// Creates a scenario from a generation request.
    /// </summary>
    /// <param name="request">The generation request.</param>
    /// <returns>A new scenario with request parameters applied.</returns>
    Scenario FromRequest(GenerateJsonStoryRequest request);
}
