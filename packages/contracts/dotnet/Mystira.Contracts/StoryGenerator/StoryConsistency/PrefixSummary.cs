using System.Text.Json.Serialization;

namespace Mystira.Contracts.StoryGenerator.StoryConsistency;

/// <summary>
/// Represents a summary of the story state up to a specific point.
/// </summary>
public class PrefixSummary
{
    /// <summary>
    /// Path ID this summary represents.
    /// </summary>
    [JsonPropertyName("path_id")]
    public string PathId { get; set; } = string.Empty;

    /// <summary>
    /// Scene IDs included in this prefix.
    /// </summary>
    [JsonPropertyName("scene_ids")]
    public List<string> SceneIds { get; set; } = new();

    /// <summary>
    /// Narrative summary of events so far.
    /// </summary>
    [JsonPropertyName("narrative_summary")]
    public string NarrativeSummary { get; set; } = string.Empty;

    /// <summary>
    /// Current state of known entities.
    /// </summary>
    [JsonPropertyName("entity_states")]
    public Dictionary<string, EntityState>? EntityStates { get; set; }

    /// <summary>
    /// Current location in the story.
    /// </summary>
    [JsonPropertyName("current_location")]
    public string? CurrentLocation { get; set; }

    /// <summary>
    /// Key facts established.
    /// </summary>
    [JsonPropertyName("established_facts")]
    public List<string>? EstablishedFacts { get; set; }

    /// <summary>
    /// Active plot threads.
    /// </summary>
    [JsonPropertyName("active_threads")]
    public List<string>? ActiveThreads { get; set; }

    /// <summary>
    /// Resolved plot threads.
    /// </summary>
    [JsonPropertyName("resolved_threads")]
    public List<string>? ResolvedThreads { get; set; }

    /// <summary>
    /// Current compass/moral values.
    /// </summary>
    [JsonPropertyName("compass_values")]
    public Dictionary<string, int>? CompassValues { get; set; }

    /// <summary>
    /// When the summary was generated.
    /// </summary>
    [JsonPropertyName("generated_at")]
    public DateTime GeneratedAt { get; set; } = DateTime.UtcNow;
}

/// <summary>
/// State of an entity at a point in the story.
/// </summary>
public class EntityState
{
    /// <summary>
    /// Entity name.
    /// </summary>
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Entity type.
    /// </summary>
    [JsonPropertyName("type")]
    public string Type { get; set; } = string.Empty;

    /// <summary>
    /// Current status (alive, dead, absent, etc.).
    /// </summary>
    [JsonPropertyName("status")]
    public string Status { get; set; } = string.Empty;

    /// <summary>
    /// Current location.
    /// </summary>
    [JsonPropertyName("location")]
    public string? Location { get; set; }

    /// <summary>
    /// Known attributes.
    /// </summary>
    [JsonPropertyName("attributes")]
    public Dictionary<string, string>? Attributes { get; set; }

    /// <summary>
    /// Last scene where entity appeared.
    /// </summary>
    [JsonPropertyName("last_seen")]
    public string? LastSeen { get; set; }
}
