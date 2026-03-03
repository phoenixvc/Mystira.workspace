using System.Text.Json.Serialization;

namespace Mystira.Contracts.StoryGenerator.Chat;

/// <summary>
/// Structured response for chat orchestration. Either contains a handler result
/// or a clarification prompt asking the user for more context/parameters.
/// </summary>
public class ChatOrchestrationResponse
{
    [JsonPropertyName("success")] public bool Success { get; set; }

    [JsonPropertyName("error")] public string? Error { get; set; }

    [JsonPropertyName("requires_clarification")]
    public bool RequiresClarification { get; set; }

    [JsonPropertyName("prompt")] public string? Prompt { get; set; }

    [JsonPropertyName("intent")] public string? Intent { get; set; }

    [JsonPropertyName("handler")] public string? Handler { get; set; }

    [JsonPropertyName("result")] public object? Result { get; set; }
}
