using System.Runtime.Serialization;
using System.Text.Json.Serialization;

namespace Mystira.Contracts.StoryGenerator.Stories;

/// <summary>
/// Defines the type of scene in a story scenario.
/// </summary>
[JsonConverter(typeof(JsonStringEnumConverter))]
public enum SceneType
{
    /// <summary>
    /// A narrative scene that advances the story through exposition.
    /// </summary>
    [JsonPropertyName("narrative")]
    [EnumMember(Value = "narrative")]
    Narrative,

    /// <summary>
    /// A choice scene where the player makes a decision that affects the story.
    /// </summary>
    [JsonPropertyName("choice")]
    [EnumMember(Value = "choice")]
    Choice,

    /// <summary>
    /// A roll scene involving dice or skill checks.
    /// </summary>
    [JsonPropertyName("roll")]
    [EnumMember(Value = "roll")]
    Roll,

    /// <summary>
    /// A special scene with unique mechanics or story significance.
    /// </summary>
    [JsonPropertyName("special")]
    [EnumMember(Value = "special")]
    Special
}
