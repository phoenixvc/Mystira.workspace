using System.Text.Json.Serialization;

namespace Mystira.Contracts.StoryGenerator.Commands.Chat;

/// <summary>
/// Command to request schema documentation.
/// </summary>
public class SchemaDocsCommand
{
    /// <summary>
    /// Specific schema element to document.
    /// </summary>
    [JsonPropertyName("element")]
    public string? Element { get; set; }

    /// <summary>
    /// Format for the documentation.
    /// </summary>
    [JsonPropertyName("format")]
    public string Format { get; set; } = "markdown";

    /// <summary>
    /// Whether to include the raw schema.
    /// </summary>
    [JsonPropertyName("include_schema")]
    public bool IncludeSchema { get; set; } = false;
}

/// <summary>
/// Response containing schema documentation.
/// </summary>
public class SchemaDocsResponse
{
    /// <summary>
    /// The documentation content.
    /// </summary>
    [JsonPropertyName("content")]
    public string Content { get; set; } = string.Empty;

    /// <summary>
    /// Schema element documented.
    /// </summary>
    [JsonPropertyName("element")]
    public string? Element { get; set; }

    /// <summary>
    /// Raw schema JSON if requested.
    /// </summary>
    [JsonPropertyName("schema")]
    public string? Schema { get; set; }

    /// <summary>
    /// Schema version.
    /// </summary>
    [JsonPropertyName("version")]
    public string? Version { get; set; }

    /// <summary>
    /// Available schema elements.
    /// </summary>
    [JsonPropertyName("available_elements")]
    public List<SchemaElement>? AvailableElements { get; set; }
}

/// <summary>
/// Information about a schema element.
/// </summary>
public class SchemaElement
{
    /// <summary>
    /// Element name.
    /// </summary>
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Element type.
    /// </summary>
    [JsonPropertyName("type")]
    public string Type { get; set; } = string.Empty;

    /// <summary>
    /// Element description.
    /// </summary>
    [JsonPropertyName("description")]
    public string? Description { get; set; }

    /// <summary>
    /// Whether the element is required.
    /// </summary>
    [JsonPropertyName("required")]
    public bool Required { get; set; }

    /// <summary>
    /// Child elements.
    /// </summary>
    [JsonPropertyName("children")]
    public List<string>? Children { get; set; }
}
