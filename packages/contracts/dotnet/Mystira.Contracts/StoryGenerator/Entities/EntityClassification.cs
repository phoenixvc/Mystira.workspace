using System.Text.Json.Serialization;

namespace Mystira.Contracts.StoryGenerator.Entities;

public class EntityClassification
{
    [JsonPropertyName("time_delta")]
    public string TimeDelta { get; set; } = "none";

    [JsonPropertyName("introduced_entities")]
    public SceneEntity[] IntroducedEntities { get; set; } = [];

    [JsonPropertyName("removed_entities")]
    public SceneEntity[] RemovedEntities { get; set; } = [];
}
