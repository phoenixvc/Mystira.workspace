namespace Mystira.Contracts.StoryGenerator.Stories;

public class GenerateStoryRequest
{
    public string Prompt { get; set; } = string.Empty;

    public string Tone { get; set; } = string.Empty;

    public int TargetLength { get; set; } = 500;
}
