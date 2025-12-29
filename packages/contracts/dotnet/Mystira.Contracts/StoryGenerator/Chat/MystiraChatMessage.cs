using System.Text.Json.Serialization;

namespace Mystira.Contracts.StoryGenerator.Chat;

public class MystiraChatMessage
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = Guid.NewGuid().ToString();

    [JsonPropertyName("messageType")]
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public ChatMessageType MessageType { get; set; }

    [JsonPropertyName("content")]
    public string Content { get; set; } = string.Empty;

    [JsonPropertyName("timestamp")]
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;

    [JsonPropertyName("metadata")]
    public Dictionary<string, object>? Metadata { get; set; }
}

public enum ChatMessageType
{
    User,
    AI,
    System
}
