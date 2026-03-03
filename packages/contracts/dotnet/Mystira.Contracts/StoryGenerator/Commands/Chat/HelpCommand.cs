using System.Text.Json.Serialization;

namespace Mystira.Contracts.StoryGenerator.Commands.Chat;

/// <summary>
/// Command to request help information.
/// </summary>
public class HelpCommand
{
    /// <summary>
    /// Optional specific topic to get help on.
    /// </summary>
    [JsonPropertyName("topic")]
    public string? Topic { get; set; }

    /// <summary>
    /// Whether to include examples.
    /// </summary>
    [JsonPropertyName("include_examples")]
    public bool IncludeExamples { get; set; } = true;

    /// <summary>
    /// Level of detail (brief, standard, detailed).
    /// </summary>
    [JsonPropertyName("detail_level")]
    public string DetailLevel { get; set; } = "standard";
}

/// <summary>
/// Response to a help command.
/// </summary>
public class HelpResponse
{
    /// <summary>
    /// The help content.
    /// </summary>
    [JsonPropertyName("content")]
    public string Content { get; set; } = string.Empty;

    /// <summary>
    /// Specific topic this help addresses.
    /// </summary>
    [JsonPropertyName("topic")]
    public string? Topic { get; set; }

    /// <summary>
    /// Available commands or topics.
    /// </summary>
    [JsonPropertyName("available_topics")]
    public List<string>? AvailableTopics { get; set; }

    /// <summary>
    /// Examples if requested.
    /// </summary>
    [JsonPropertyName("examples")]
    public List<HelpExample>? Examples { get; set; }

    /// <summary>
    /// Related topics.
    /// </summary>
    [JsonPropertyName("related_topics")]
    public List<string>? RelatedTopics { get; set; }
}

/// <summary>
/// An example in help documentation.
/// </summary>
public class HelpExample
{
    /// <summary>
    /// Title of the example.
    /// </summary>
    [JsonPropertyName("title")]
    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// Example input.
    /// </summary>
    [JsonPropertyName("input")]
    public string Input { get; set; } = string.Empty;

    /// <summary>
    /// Expected output or result.
    /// </summary>
    [JsonPropertyName("output")]
    public string? Output { get; set; }

    /// <summary>
    /// Description of the example.
    /// </summary>
    [JsonPropertyName("description")]
    public string? Description { get; set; }
}
