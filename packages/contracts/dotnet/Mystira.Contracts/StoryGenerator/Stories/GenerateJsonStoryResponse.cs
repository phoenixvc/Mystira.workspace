namespace Mystira.Contracts.StoryGenerator.Stories;

public class GenerateJsonStoryResponse
{
    public bool Success { get; set; }
    /// <summary>
    /// The backend now generates JSON. UI is responsible for converting JSON to YAML for display.
    /// </summary>
    public string Json { get; set; } = string.Empty;
    public string Provider { get; set; } = string.Empty;
    public string Model { get; set; } = string.Empty;
    public string? ModelId { get; set; }
    public string? Error { get; set; }
    public bool IsIncomplete { get; set; }
}
