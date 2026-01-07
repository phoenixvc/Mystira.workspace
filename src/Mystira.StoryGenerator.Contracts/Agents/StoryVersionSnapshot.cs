namespace Mystira.StoryGenerator.Domain.Agents;

/// <summary>
/// Represents an immutable snapshot of a story version at a point in time.
/// </summary>
public class StoryVersionSnapshot
{
    /// <summary>
    /// The version number (1-based).
    /// </summary>
    public int VersionNumber { get; set; }

    /// <summary>
    /// The story content as JSON string.
    /// </summary>
    public string StoryJson { get; set; } = string.Empty;

    /// <summary>
    /// Timestamp when this version was created.
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// The session stage when this version was created.
    /// </summary>
    public string StageWhenCreated { get; set; } = string.Empty;

    /// <summary>
    /// The iteration number when this version was created.
    /// </summary>
    public int IterationNumber { get; set; }
}
