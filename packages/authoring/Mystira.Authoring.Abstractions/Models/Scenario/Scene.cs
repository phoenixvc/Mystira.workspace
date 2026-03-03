using System.ComponentModel.DataAnnotations;
using Mystira.Domain.Enums;
using Mystira.Domain.Models;
using DomainMediaReferences = Mystira.Domain.Models.MediaReferences;

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
    public DomainMediaReferences? Media { get; set; }

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
