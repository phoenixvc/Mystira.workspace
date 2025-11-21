namespace Mystira.StoryGenerator.Api.Services.Instructions;

public class InstructionSearchContext
{
    public string QueryText { get; set; } = string.Empty;
    public IReadOnlyCollection<string>? Categories { get; set; }
    public IReadOnlyCollection<string>? InstructionTypes { get; set; }
    public int? TopK { get; set; }
}
