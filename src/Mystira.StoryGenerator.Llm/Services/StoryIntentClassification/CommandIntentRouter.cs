using Microsoft.Extensions.Logging;
using Mystira.StoryGenerator.Domain.Services;

namespace Mystira.StoryGenerator.Llm.Services.StoryIntentClassification;

public class CommandIntentRouter : ICommandRouter
{
    private readonly ILlmIntentLlmClassificationService _llmIntentLlmClassification;
    private readonly ILogger<CommandIntentRouter> _logger;

    public CommandIntentRouter(
        ILlmIntentLlmClassificationService llmIntentLlmClassification,
        ILogger<CommandIntentRouter> logger)
    {
        _llmIntentLlmClassification = llmIntentLlmClassification;
        _logger = logger;
    }

    public async Task<string?> DetectPrimaryInstructionTypeAsync(string userQuery, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(userQuery))
        {
            _logger.LogWarning("DetectPrimaryInstructionTypeAsync called with empty query");
            return null;
        }

        var classification = await _llmIntentLlmClassification.ClassifyAsync(userQuery, cancellationToken);
        if (classification == null || classification.InstructionTypes.Length == 0)
        {
            _logger.LogWarning("Intent classification returned no instruction types for query: {Query}", userQuery);
            return null;
        }

        return classification.InstructionTypes[0];
    }
}
