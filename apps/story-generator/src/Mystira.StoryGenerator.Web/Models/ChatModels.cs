namespace Mystira.StoryGenerator.Web.Models;

public class StoryGeneratorChatHistory
{
    public string Id { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.Now;
    public List<StoryGeneratorChatMessage> Messages { get; set; } = new();
    public string? YamlSnapshot { get; set; }
    public List<YamlSnapshot> YamlSnapshots { get; set; } = new();
}

public class YamlSnapshot
{
    public int Version { get; set; }
    public string Content { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

public class StoryGeneratorChatMessage
{
    public string Content { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; } = DateTime.Now;
}

public class YamlValidationResult
{
    public bool IsValid { get; set; }
    public List<string> Errors { get; set; } = new();
}
