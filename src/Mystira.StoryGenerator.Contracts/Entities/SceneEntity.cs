using System.Text.Json.Serialization;

namespace Mystira.StoryGenerator.Contracts.Entities;

public class SceneEntity
{
    public SceneEntityType Type { get; set; }
    public string Name { get; set; } = string.Empty;
    [JsonPropertyName("is_proper_noun")] public bool IsProperNoun { get; set; } = false;
    public Confidence Confidence { get; set; } = Confidence.Unknown;
}
