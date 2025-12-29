using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Mystira.Contracts.StoryGenerator.Chat;

/// <summary>
/// Request model for chat completion API calls
/// </summary>
public class ChatCompletionRequest
{
    /// <summary>
    /// Optional AI provider override (e.g., "azure-openai", "google-gemini").
    /// When omitted, the server resolves the provider using the configured model bindings.
    /// </summary>
    [JsonPropertyName("provider")]
    public string? Provider { get; set; }

    /// <summary>
    /// Optional logical model identifier defined in configuration (e.g., "story-advanced").
    /// </summary>
    [JsonPropertyName("model_id")]
    public string? ModelId { get; set; }

    /// <summary>
    /// Provider-specific model name or deployment (e.g., "gpt-4o", "gemini-pro").
    /// </summary>
    [JsonPropertyName("model")]
    public string? Model { get; set; }

    /// <summary>
    /// Array of messages in the conversation
    /// </summary>
    [JsonPropertyName("messages")]
    [Required]
    [MinLength(1)]
    public List<MystiraChatMessage> Messages { get; set; } = new();

    /// <summary>
    /// Controls randomness in responses (0.0 to 2.0)
    /// </summary>
    [JsonPropertyName("temperature")]
    [Range(0.0, 2.0)]
    public double Temperature { get; set; } = 0.7;

    /// <summary>
    /// Maximum number of tokens to generate (up to 4096)
    /// </summary>
    [JsonPropertyName("max_tokens")]
    [Range(1, 4096)]
    public int MaxTokens { get; set; } = 1000;

    /// <summary>
    /// Optional system prompt to set behavior
    /// </summary>
    [JsonPropertyName("system_prompt")]
    public string? SystemPrompt { get; set; }

    /// <summary>
    /// Optional: When provided and supported by the provider, requests the model to format output
    /// according to the given JSON Schema using the provider's native response format feature.
    /// Currently supported by Azure OpenAI in this solution.
    /// </summary>
    [JsonPropertyName("json_schema_format")]
    public JsonSchemaResponseFormat? JsonSchemaFormat { get; set; }

    /// <summary>
    /// Whether the schema should be enforced strictly by the provider, if supported.
    /// </summary>
    public bool? IsSchemaValidationStrict { get; set; }

    /// <summary>
    /// Optional snapshot of the current story being worked on
    /// </summary>
    [JsonPropertyName("current_story")]
    public StorySnapshot? CurrentStory { get; set; }
}

/// <summary>
/// Describes a JSON Schema response format request for providers that support schema-constrained output.
/// </summary>
public class JsonSchemaResponseFormat
{
    /// <summary>
    /// A short, descriptive name for the schema format. Used by some providers to label the format.
    /// </summary>
    [JsonPropertyName("format_name")]
    public string FormatName { get; set; } = "mystira-json";

    /// <summary>
    /// The JSON Schema as a raw JSON string.
    /// </summary>
    [JsonPropertyName("schema_json")]
    public string SchemaJson { get; set; } = string.Empty;

    /// <summary>
    /// Whether the schema should be enforced strictly by the provider, if supported.
    /// </summary>
    [JsonPropertyName("is_strict")]
    public bool IsStrict { get; set; } = true;
}
