using Mystira.StoryGenerator.Contracts.Models;
using Mystira.StoryGenerator.Domain.Agents;

namespace Mystira.StoryGenerator.Api.Models;

/// <summary>
/// Response model for session state.
/// </summary>
public class SessionStateResponse
{
    public string SessionId { get; set; } = string.Empty;
    public string ThreadId { get; set; } = string.Empty;
    public string Stage { get; set; } = string.Empty;
    public int IterationCount { get; set; }
    public double CostEstimate { get; set; }
    public string CurrentStoryJson { get; set; } = string.Empty;
    public string CurrentStoryYaml { get; set; } = string.Empty;
    public EvaluationReport? LastEvaluationReport { get; set; }
    public List<StoryVersionSnapshot> StoryVersions { get; set; } = new();
    public string? ErrorMessage { get; set; }
}
