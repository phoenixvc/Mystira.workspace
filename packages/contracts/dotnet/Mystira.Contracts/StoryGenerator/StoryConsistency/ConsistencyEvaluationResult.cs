using System.Text.Json.Serialization;

namespace Mystira.Contracts.StoryGenerator.StoryConsistency;

public class ConsistencyEvaluationResult
{
    [JsonPropertyName("overall_assessment")]
    public string OverallAssessment { get; set; } = string.Empty; // ok | has_minor_issues | has_major_issues | broken

    [JsonPropertyName("issues")]
    public List<ConsistencyIssue> Issues { get; set; } = new();
}

public class ConsistencyIssue
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    [JsonPropertyName("severity")]
    public string Severity { get; set; } = string.Empty; // low | medium | high | critical

    [JsonPropertyName("category")]
    public string Category { get; set; } = string.Empty; // entity_consistency | time_consistency | emotional_consistency | causal_consistency | other

    [JsonPropertyName("scene_ids")]
    public List<string> SceneIds { get; set; } = new();

    [JsonPropertyName("summary")]
    public string Summary { get; set; } = string.Empty;

    [JsonPropertyName("details")]
    public string Details { get; set; } = string.Empty;

    [JsonPropertyName("suggested_fix")]
    public string? SuggestedFix { get; set; }
}

