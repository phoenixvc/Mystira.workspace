using System.Text.Json.Serialization;
using Mystira.StoryGenerator.Contracts.Entities;

namespace Mystira.StoryGenerator.Domain.Services;

public sealed class ScenarioPathPrefixSummary
{
    /// <summary>
    /// Ordered list of scene IDs that make up this front-merged prefix.
    /// This should correspond to the path prefix the LLM saw when generating the summary.
    /// </summary>
    [JsonPropertyName("path_scene_ids")]
    public List<string> PathSceneIds { get; set; } = new();

    /// <summary>
    /// Entities that the prefix strongly suggests MUST exist and be “known” at the end of this prefix
    /// across all merged branches (high confidence).
    /// </summary>
    [JsonPropertyName("definitely_present_entities")]
    public List<SceneEntity> DefinitelyPresentEntities { get; set; } = new();

    /// <summary>
    /// Entities that MAY exist at the end of this prefix (present in some branches, uncertain in others).
    /// These are candidates for extra checking downstream.
    /// </summary>
    [JsonPropertyName("maybe_present_entities")]
    public List<SceneEntity> MaybePresentEntities { get; set; } = new();

    /// <summary>
    /// Entities that the prefix strongly suggests are definitely absent
    /// (e.g., explicitly removed, dead, destroyed, left behind, etc.).
    /// </summary>
    [JsonPropertyName("definitely_absent_entities")]
    public List<SceneEntity> DefinitelyAbsentEntities { get; set; } = new();

    /// <summary>
    /// Free-form notes or warnings from the LLM about possible inconsistencies,
    /// timeline issues, or branch-sensitive assumptions.
    /// </summary>
    [JsonPropertyName("notes")]
    public List<string> Notes { get; set; } = new();
}
