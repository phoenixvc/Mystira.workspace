using System.ComponentModel.DataAnnotations;

namespace Mystira.StoryGenerator.Contracts.Models;

/// <summary>
/// Request for refining a story generation session.
/// </summary>
public class RefineRequest
{
    /// <summary>
    /// List of scene IDs to target for refinement. If empty, all scenes are refined.
    /// </summary>
    public List<string> TargetSceneIds { get; set; } = new();

    /// <summary>
    /// List of aspects to focus on (e.g., "character_voice", "pacing", "dialogue").
    /// </summary>
    public List<string> Aspects { get; set; } = new();

    /// <summary>
    /// Additional constraints or requirements from the user.
    /// </summary>
    [MaxLength(1000)]
    public string? Constraints { get; set; }
}
