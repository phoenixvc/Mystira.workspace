namespace Mystira.StoryGenerator.Contracts.Stories;

public class GenerateYamlStoryResponse
{
    public bool Success { get; set; }
    public string Yaml { get; set; } = string.Empty;
    public string Provider { get; set; } = string.Empty;
    public string Model { get; set; } = string.Empty;
    public string? ModelId { get; set; }
    public string? Error { get; set; }
}
