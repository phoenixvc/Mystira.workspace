using System.Text.Json.Serialization;

namespace Mystira.Contracts.StoryGenerator.Entities;

/// <summary>
/// Entity that was introduced in a specific scene.
/// </summary>
public class SceneEntity
{
    public SceneEntityType Type { get; set; }
    public string Name { get; set; } = string.Empty;
    [JsonPropertyName("is_proper_noun")] public bool IsProperNoun { get; set; } = false;
    public Confidence Confidence { get; set; } = Confidence.Unknown;
}
