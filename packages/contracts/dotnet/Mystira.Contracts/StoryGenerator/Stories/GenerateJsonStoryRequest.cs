namespace Mystira.Contracts.StoryGenerator.Stories;

public class GenerateJsonStoryRequest
{
    public string? Provider { get; set; }
    public string? ModelId { get; set; }
    public string? Model { get; set; }

    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string Difficulty { get; set; } = "Medium";
    public string SessionLength { get; set; } = "Medium";
    public string AgeGroup { get; set; } = string.Empty; // e.g., "10-12"
    public int MinimumAge { get; set; }

    public List<string> CoreAxes { get; set; } = new();
    public List<string> Archetypes { get; set; } = new();
    public List<string>? Tags { get; set; }
    public string? Tone { get; set; }

    public int MinScenes { get; set; } = 6;
    public int MaxScenes { get; set; } = 12;

    /// <summary>
    /// Number of player characters to generate in the YAML characters section
    /// </summary>
    public int CharacterCount { get; set; } = 0;
}
