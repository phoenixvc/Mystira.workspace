namespace Mystira.StoryGenerator.Domain.Services;

public interface IInstructionBlockService
{
    Task<string?> BuildInstructionBlockAsync(InstructionSearchContext context, CancellationToken cancellationToken = default);
}

public class InstructionSearchContext
{
    public string QueryText { get; set; } = string.Empty;
    public string[] Categories { get; set; } = Array.Empty<string>();
    public string[] InstructionTypes { get; set; } = Array.Empty<string>();
    public int TopK { get; set; } = 8;
}
