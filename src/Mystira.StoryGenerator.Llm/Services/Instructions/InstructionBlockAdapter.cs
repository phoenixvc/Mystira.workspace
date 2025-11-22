using Mystira.StoryGenerator.Domain.Services;

namespace Mystira.StoryGenerator.Llm.Services.Instructions;

/// <summary>
/// Adapter to expose the API InstructionBlockService via the Domain IInstructionBlockService interface.
/// Maps between slightly different InstructionSearchContext shapes.
/// </summary>
public class InstructionBlockAdapter : IInstructionBlockService
{
    private readonly IInstructionBlockService _inner;

    public InstructionBlockAdapter(IInstructionBlockService inner)
    {
        _inner = inner;
    }

    public async Task<string?> BuildInstructionBlockAsync(InstructionSearchContext context, CancellationToken cancellationToken = default)
    {
        var apiContext = new InstructionSearchContext
        {
            QueryText = context.QueryText,
            Categories = context.Categories,
            InstructionTypes = context.InstructionTypes,
            TopK = context.TopK
        };

        return await _inner.BuildInstructionBlockAsync(apiContext, cancellationToken);
    }
}
