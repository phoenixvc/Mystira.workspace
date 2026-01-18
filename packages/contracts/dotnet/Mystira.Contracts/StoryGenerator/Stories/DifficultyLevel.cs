using System.Runtime.Serialization;
using System.Text.Json.Serialization;

namespace Mystira.Contracts.StoryGenerator.Stories;

/// <summary>
/// Defines the difficulty level of a story scenario.
/// </summary>
[JsonConverter(typeof(JsonStringEnumConverter))]
public enum DifficultyLevel
{
    /// <summary>
    /// Easy difficulty with simpler challenges and more forgiving outcomes.
    /// </summary>
    [JsonPropertyName("easy")]
    [EnumMember(Value = "easy")]
    Easy,

    /// <summary>
    /// Medium difficulty with balanced challenges.
    /// </summary>
    [JsonPropertyName("medium")]
    [EnumMember(Value = "medium")]
    Medium,

    /// <summary>
    /// Hard difficulty with complex challenges and higher stakes.
    /// </summary>
    [JsonPropertyName("hard")]
    [EnumMember(Value = "hard")]
    Hard
}
