namespace Mystira.StoryGenerator.Api.Models;

/// <summary>
/// Response for refining a story generation session.
/// </summary>
public class RefineResponse
{
    /// <summary>
    /// The session identifier.
    /// </summary>
    public string SessionId { get; set; } = string.Empty;

    /// <summary>
    /// Current stage of the session.
    /// </summary>
    public string Stage { get; set; } = string.Empty;

    /// <summary>
    /// Current iteration count.
    /// </summary>
    public int IterationCount { get; set; }

    /// <summary>
    /// Preview of the refined story (first 500 characters).
    /// </summary>
    public string RefinedStoryPreview { get; set; } = string.Empty;
}