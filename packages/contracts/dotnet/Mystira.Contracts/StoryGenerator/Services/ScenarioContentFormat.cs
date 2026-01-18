using System.Runtime.Serialization;
using System.Text.Json.Serialization;

namespace Mystira.Contracts.StoryGenerator.Services;

/// <summary>
/// Format options for scenario content.
/// </summary>
[JsonConverter(typeof(JsonStringEnumConverter))]
public enum ScenarioContentFormat
{
    /// <summary>
    /// JSON format.
    /// </summary>
    [JsonPropertyName("json")]
    [EnumMember(Value = "json")]
    Json,

    /// <summary>
    /// YAML format.
    /// </summary>
    [JsonPropertyName("yaml")]
    [EnumMember(Value = "yaml")]
    Yaml,

    /// <summary>
    /// Markdown format (for human-readable output).
    /// </summary>
    [JsonPropertyName("markdown")]
    [EnumMember(Value = "markdown")]
    Markdown,

    /// <summary>
    /// Plain text format.
    /// </summary>
    [JsonPropertyName("text")]
    [EnumMember(Value = "text")]
    Text
}
