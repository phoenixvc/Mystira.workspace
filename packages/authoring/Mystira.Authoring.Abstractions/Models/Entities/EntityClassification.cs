namespace Mystira.Authoring.Abstractions.Models.Entities;

/// <summary>
/// Result of entity classification by the LLM.
/// </summary>
public class EntityClassification
{
    /// <summary>
    /// Entities classified from the scene.
    /// </summary>
    public List<SceneEntity> Entities { get; set; } = new();

    /// <summary>
    /// Scene ID this classification is for.
    /// </summary>
    public string SceneId { get; set; } = string.Empty;

    /// <summary>
    /// Whether the classification was successful.
    /// </summary>
    public bool Success { get; set; }

    /// <summary>
    /// Error message if classification failed.
    /// </summary>
    public string? Error { get; set; }
}
