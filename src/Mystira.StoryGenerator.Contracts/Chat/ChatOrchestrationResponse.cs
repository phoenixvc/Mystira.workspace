using System.Text.Json.Serialization;
using Mystira.StoryGenerator.Contracts.Stories;

namespace Mystira.StoryGenerator.Contracts.Chat;

public class ChatOrchestrationResponse
{
    [JsonPropertyName("success")]
    public bool Success { get; set; }

    [JsonPropertyName("error")]
    public string? Error { get; set; }

    [JsonPropertyName("requiresClarification")]
    public bool RequiresClarification { get; set; }

    [JsonPropertyName("prompt")]
    public string? Prompt { get; set; }

    [JsonPropertyName("intent")]
    public string? Intent { get; set; }

    [JsonPropertyName("handler")]
    public string? Handler { get; set; }

    [JsonPropertyName("result")]
    public object? Result { get; set; }
}