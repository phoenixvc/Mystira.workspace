using System.Text.Json.Serialization;

namespace Mystira.Contracts.StoryGenerator.Entities;

/// <summary>
/// Represents a graph of entities and their relationships in a story.
/// </summary>
public class EntityGraph
{
    /// <summary>
    /// Unique identifier for this graph.
    /// </summary>
    [JsonPropertyName("id")]
    public string Id { get; set; } = Guid.NewGuid().ToString();

    /// <summary>
    /// Scenario ID this graph represents.
    /// </summary>
    [JsonPropertyName("scenario_id")]
    public string? ScenarioId { get; set; }

    /// <summary>
    /// All entities in the graph.
    /// </summary>
    [JsonPropertyName("entities")]
    public List<StoryEntity> Entities { get; set; } = new();

    /// <summary>
    /// All relationships in the graph.
    /// </summary>
    [JsonPropertyName("relationships")]
    public List<EntityRelationship> Relationships { get; set; } = new();

    /// <summary>
    /// All mentions by scene.
    /// </summary>
    [JsonPropertyName("mentions")]
    public Dictionary<string, List<EntityMention>>? MentionsByScene { get; set; }

    /// <summary>
    /// When the graph was generated.
    /// </summary>
    [JsonPropertyName("generated_at")]
    public DateTime GeneratedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Gets an entity by ID.
    /// </summary>
    public StoryEntity? GetEntity(string id) =>
        Entities.FirstOrDefault(e => e.Id == id);

    /// <summary>
    /// Gets an entity by name.
    /// </summary>
    public StoryEntity? GetEntityByName(string name) =>
        Entities.FirstOrDefault(e =>
            e.Name.Equals(name, StringComparison.OrdinalIgnoreCase) ||
            (e.Aliases?.Any(a => a.Equals(name, StringComparison.OrdinalIgnoreCase)) == true));

    /// <summary>
    /// Gets relationships for an entity.
    /// </summary>
    public IEnumerable<EntityRelationship> GetRelationships(string entityId) =>
        Relationships.Where(r => r.SourceId == entityId || r.TargetId == entityId);

    /// <summary>
    /// Gets entities by type.
    /// </summary>
    public IEnumerable<StoryEntity> GetEntitiesByType(EntityType type) =>
        Entities.Where(e => e.Type == type);
}
