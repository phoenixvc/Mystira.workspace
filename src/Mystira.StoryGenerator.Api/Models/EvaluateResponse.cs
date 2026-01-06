using Mystira.StoryGenerator.Domain.Agents;

namespace Mystira.StoryGenerator.Api.Models;

/// <summary>
/// Response for evaluating a story generation session.
/// </summary>
public class EvaluateResponse
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
    /// The evaluation report.
    /// </summary>
    public EvaluationReport EvaluationReport { get; set; } = new();

    /// <summary>
    /// Recommended action based on evaluation.
    /// </summary>
    public string RecommendedAction { get; set; } = string.Empty;
}