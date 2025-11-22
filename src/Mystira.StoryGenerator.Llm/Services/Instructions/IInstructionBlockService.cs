using Mystira.StoryGenerator.Domain.Services;

namespace Mystira.StoryGenerator.Llm.Services.Instructions;

public interface IInstructionBlockService
{
    Task<string?> BuildInstructionBlockAsync(InstructionSearchContext context, CancellationToken cancellationToken = default);
}
