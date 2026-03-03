using System.Text.Json.Serialization;

namespace Mystira.Contracts.StoryGenerator.Entities;

public enum Confidence
{
    [JsonPropertyName("unknown")]
    Unknown,

    [JsonPropertyName("low")]
    Low,

    [JsonPropertyName("medium")]
    Medium,

    [JsonPropertyName("high")]
    High
}
