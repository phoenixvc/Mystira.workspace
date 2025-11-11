namespace Mystira.StoryGenerator.Contracts.Stories;

public class RandomStoryParametersRequest
{
    // Optional LLM provider to use (falls back to default if null/empty)
    public string? Provider { get; set; }
    public string? ModelId { get; set; }
    public string? Model { get; set; }

    // Optional theme or seed idea to bias the randomization
    public string? Theme { get; set; }

    // Difficulty and pacing hints
    public string? Difficulty { get; set; }
    public string? SessionLength { get; set; }

    // Optional overrides/constraints
    public int? MinimumAge { get; set; }
    public string? AgeGroup { get; set; }
    public int? MinScenes { get; set; }
    public int? MaxScenes { get; set; }
}
