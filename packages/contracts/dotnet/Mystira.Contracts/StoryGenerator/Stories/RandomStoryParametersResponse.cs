namespace Mystira.Contracts.StoryGenerator.Stories;

public class RandomStoryParametersResponse
{
    public bool Success { get; set; }
    public string? Error { get; set; }

    // Proposed parameters
    public string Title { get; set; } = string.Empty;
    public string Difficulty { get; set; } = "Medium";
    public string SessionLength { get; set; } = "Medium";
    public string AgeGroup { get; set; } = string.Empty; // e.g., "8-10"
    public int MinimumAge { get; set; }
    public List<string> CoreAxes { get; set; } = new();
    public List<string> Archetypes { get; set; } = new();
    public int CharacterCount { get; set; }
    public int MinScenes { get; set; } = 6;
    public int MaxScenes { get; set; } = 12;
    public List<string>? Tags { get; set; }
    public string? Tone { get; set; }

    /// <summary>
    /// Narrative description to present to the user before proceeding
    /// </summary>
    public string Description { get; set; } = string.Empty;

    // Info
    public string Provider { get; set; } = string.Empty;
    public string Model { get; set; } = string.Empty;
    public string? ModelId { get; set; }
}
