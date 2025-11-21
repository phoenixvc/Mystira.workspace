namespace Mystira.StoryGenerator.Api.Services.Instructions;

public class InstructionChunk
{
    public string? Id { get; set; }
    public string Content { get; set; } = string.Empty;
    public string? Category { get; set; }
    public string? InstructionType { get; set; }
    public bool IsMandatory { get; set; }
    public int? Order { get; set; }
    public double? Score { get; set; }
    public IReadOnlyList<string> Tags { get; set; } = Array.Empty<string>();
}
