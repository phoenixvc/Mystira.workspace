using System.Runtime.Serialization;
using System.Text.Json.Serialization;

namespace Mystira.Contracts.StoryGenerator.Stories;

/// <summary>
/// Defines the expected session length for a story scenario.
/// </summary>
[JsonConverter(typeof(JsonStringEnumConverter))]
public enum SessionLength
{
    /// <summary>
    /// Short session typically lasting 15-30 minutes with 3-5 scenes.
    /// </summary>
    [JsonPropertyName("short")]
    [EnumMember(Value = "short")]
    Short,

    /// <summary>
    /// Medium session typically lasting 30-60 minutes with 6-10 scenes.
    /// </summary>
    [JsonPropertyName("medium")]
    [EnumMember(Value = "medium")]
    Medium,

    /// <summary>
    /// Long session typically lasting 60+ minutes with 10+ scenes.
    /// </summary>
    [JsonPropertyName("long")]
    [EnumMember(Value = "long")]
    Long
}
