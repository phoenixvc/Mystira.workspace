using System.Text.Json.Serialization;

namespace Mystira.Contracts.StoryGenerator.Chat;

public class ChatSession
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = Guid.NewGuid().ToString();

    [JsonPropertyName("title")]
    public string Title { get; set; } = string.Empty;

    [JsonPropertyName("createdAt")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [JsonPropertyName("updatedAt")]
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    [JsonPropertyName("messages")]
    public List<MystiraChatMessage> Messages { get; set; } = new();

    [JsonPropertyName("yamlSnapshot")]
    public string? YamlSnapshot { get; set; }

    [JsonPropertyName("providerSettings")]
    public ProviderSettings? ProviderSettings { get; set; }

    [JsonIgnore]
    public bool HasYamlSnapshot => !string.IsNullOrEmpty(YamlSnapshot);
}

public class ProviderSettings
{
    [JsonPropertyName("provider")]
    public string Provider { get; set; } = string.Empty;

    [JsonPropertyName("model")]
    public string Model { get; set; } = string.Empty;

    [JsonPropertyName("apiKey")]
    public string? ApiKey { get; set; }

    [JsonPropertyName("temperature")]
    public float Temperature { get; set; } = 0.7f;

    [JsonPropertyName("maxTokens")]
    public int MaxTokens { get; set; } = 1000;

    [JsonPropertyName("additionalSettings")]
    public Dictionary<string, object>? AdditionalSettings { get; set; }
}

public class SessionMetadata
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    [JsonPropertyName("title")]
    public string Title { get; set; } = string.Empty;

    [JsonPropertyName("createdAt")]
    public DateTime CreatedAt { get; set; }

    [JsonPropertyName("updatedAt")]
    public DateTime UpdatedAt { get; set; }

    [JsonPropertyName("messageCount")]
    public int MessageCount { get; set; }

    [JsonPropertyName("hasYamlSnapshot")]
    public bool HasYamlSnapshot { get; set; }
}
