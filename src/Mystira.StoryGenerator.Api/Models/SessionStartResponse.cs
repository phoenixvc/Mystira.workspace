namespace Mystira.StoryGenerator.Api.Models;

/// <summary>
/// Response for starting a story generation session.
/// </summary>
public class SessionStartResponse
{
    /// <summary>
    /// Unique identifier for the created session.
    /// </summary>
    public string SessionId { get; set; } = string.Empty;

    /// <summary>
    /// Azure AI Foundry thread ID.
    /// </summary>
    public string ThreadId { get; set; } = string.Empty;

    /// <summary>
    /// Knowledge retrieval mode.
    /// </summary>
    public string KnowledgeMode { get; set; } = string.Empty;

    /// <summary>
    /// Current stage of the session.
    /// </summary>
    public string Stage { get; set; } = string.Empty;

    /// <summary>
    /// When the session was created.
    /// </summary>
    public DateTime CreatedAt { get; set; }
}