using Mystira.StoryGenerator.Domain.Agents;

namespace Mystira.StoryGenerator.Contracts.Models;

/// <summary>
/// Request for evaluating a story generation session.
/// </summary>
public class EvaluateRequest
{
    /// <summary>
    /// Optional timeout for the evaluation in seconds. Default is 600 seconds (10 minutes).
    /// </summary>
    public int? TimeoutSeconds { get; set; } = 600;
}
