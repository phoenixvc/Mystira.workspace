using System.Text.Json.Serialization;

namespace Mystira.StoryGenerator.Contracts.Entities;

public class EntityClassification
{
    // New response contract
    [JsonPropertyName("time_delta")]
    public string TimeDelta { get; set; } = "none";

    [JsonPropertyName("introduced_entities")]
    public SceneEntity[] IntroducedEntities { get; set; } = [];

    [JsonPropertyName("removed_entities")]
    public SceneEntity[] RemovedEntities { get; set; } = [];
}
