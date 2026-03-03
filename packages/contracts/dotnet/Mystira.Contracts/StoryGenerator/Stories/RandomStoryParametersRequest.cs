namespace Mystira.Contracts.StoryGenerator.Stories;

public class RandomStoryParametersRequest
{
    /// <summary>
    /// Optional LLM provider to use (falls back to default if null/empty)
    /// </summary>
    public string? Provider { get; set; }
    public string? ModelId { get; set; }
    public string? Model { get; set; }

    /// <summary>
    /// Optional theme or seed idea to bias the randomization
    /// </summary>
    public string? Theme { get; set; }

    /// <summary>
    /// Difficulty and pacing hints
    /// </summary>
    public string? Difficulty { get; set; }
    public string? SessionLength { get; set; }

    /// <summary>
    /// Optional overrides/constraints
    /// </summary>
    public int? MinimumAge { get; set; }
    public string? AgeGroup { get; set; }
    public int? MinScenes { get; set; }
    public int? MaxScenes { get; set; }
}
