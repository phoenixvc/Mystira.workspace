namespace Mystira.StoryGenerator.Contracts.Chat;

public class StorySnapshot
{
    public string StoryId { get; set; } = string.Empty;
    public int StoryVersion { get; set; }
    public string Content { get; set; } = string.Empty;
}
