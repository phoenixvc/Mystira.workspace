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

    // Backward-compat field: some callers still expect a flat list
    // of entities. We keep it and generally populate it with
    // IntroducedEntities in the classifier.
    public SceneEntity[] Entities { get; set; } = [];
}
