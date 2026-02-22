using System.Text.Json.Serialization;

namespace Mystira.StoryGenerator.Contracts.Entities;

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
