using System.ComponentModel.DataAnnotations;

namespace Mystira.Authoring.Abstractions.Models.Scenario;

/// <summary>
/// Represents a scene in an interactive story.
/// </summary>
public class Scene
{
    /// <summary>
    /// Unique identifier for the scene.
    /// </summary>
    [Required]
    public string Id { get; set; } = string.Empty;

    /// <summary>
    /// Title of the scene.
    /// </summary>
    [Required]
    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// Type of scene interaction.
    /// </summary>
    [Required]
    public SceneType Type { get; set; }

    /// <summary>
    /// Narrative description of the scene.
    /// </summary>
    [Required]
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// ID of the next scene (for linear progression).
    /// </summary>
    public string? NextSceneId { get; set; }

    /// <summary>
    /// Media references for the scene.
    /// </summary>
    public MediaReferences? Media { get; set; }

    /// <summary>
    /// Branching choices available in this scene.
    /// </summary>
    public List<Branch> Branches { get; set; } = new();

    /// <summary>
    /// Echo reveals triggered by this scene.
    /// </summary>
    public List<EchoReveal> EchoReveals { get; set; } = new();

    /// <summary>
    /// Optional difficulty rating for this scene.
    /// </summary>
    public int? Difficulty { get; set; }
}

/// <summary>
/// Type of scene interaction.
/// </summary>
public enum SceneType
{
    /// <summary>
    /// Narrative-only scene with no player interaction.
    /// </summary>
    Narrative = 0,

    /// <summary>
    /// Scene presenting player choices.
    /// </summary>
    Choice = 1,

    /// <summary>
    /// Scene with dice roll or chance mechanics.
    /// </summary>
    Roll = 2,

    /// <summary>
    /// Special scene type (puzzle, minigame, etc.).
    /// </summary>
    Special = 3
}

/// <summary>
/// Media references for a scene.
/// </summary>
public class MediaReferences
{
    /// <summary>
    /// Path or URL to image asset.
    /// </summary>
    public string? Image { get; set; }

    /// <summary>
    /// Path or URL to audio asset.
    /// </summary>
    public string? Audio { get; set; }

    /// <summary>
    /// Path or URL to video asset.
    /// </summary>
    public string? Video { get; set; }
}
