namespace Mystira.StoryGenerator.Domain.Agents;

/// <summary>
/// Represents the user's focus areas for story refinement.
/// </summary>
public class UserRefinementFocus
{
    /// <summary>
    /// List of scene IDs to target for refinement.
    /// </summary>
    public List<string> TargetSceneIds { get; set; } = new();

    /// <summary>
    /// List of aspects to focus on (e.g., "character_voice", "pacing", "dialogue").
    /// </summary>
    public List<string> Aspects { get; set; } = new();

    /// <summary>
    /// Additional constraints or requirements from the user.
    /// </summary>
    public string Constraints { get; set; } = string.Empty;

    /// <summary>
    /// Whether the user wants a full rewrite instead of targeted refinements.
    /// </summary>
    public bool IsFullRewrite { get; set; }
}
