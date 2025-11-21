namespace Mystira.StoryGenerator.Api.Services.Instructions;

public interface IInstructionBlockService
{
    Task<string?> BuildInstructionBlockAsync(InstructionSearchContext context, CancellationToken cancellationToken = default);
}
