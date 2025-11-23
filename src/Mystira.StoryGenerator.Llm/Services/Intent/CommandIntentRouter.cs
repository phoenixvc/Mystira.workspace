using Microsoft.Extensions.Logging;
using Mystira.StoryGenerator.Domain.Commands.Stories;
using Mystira.StoryGenerator.Domain.Services;

namespace Mystira.StoryGenerator.Llm.Services.Intent;

public class CommandIntentRouter : ICommandIntentRouter
{
    private readonly IIntentClassificationService _intentClassification;
    private readonly ILogger<CommandIntentRouter> _logger;

    public CommandIntentRouter(
        IIntentClassificationService intentClassification,
        ILogger<CommandIntentRouter> logger)
    {
        _intentClassification = intentClassification;
        _logger = logger;
    }

    public async Task<object?> RouteIntentToCommandAsync(string userQuery, object? context = null, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(userQuery))
        {
            _logger.LogWarning("RouteIntentToCommandAsync called with empty query");
            return null;
        }

        var classification = await _intentClassification.ClassifyIntentAsync(userQuery, cancellationToken);
        if (classification == null || classification.InstructionTypes.Length == 0)
        {
            _logger.LogWarning("Intent classification returned no instruction types for query: {Query}", userQuery);
            return null;
        }

        var instructionType = classification.InstructionTypes[0];

        _logger.LogInformation(
            "Routing intent to command: category={Category}, instructionType={InstructionType}",
            classification.Categories[0],
            instructionType);

        return instructionType switch
        {
            "story_generate_initial" => context as GenerateStoryCommand,
            "story_generate_refine" => context as RefineStoryCommand,
            "story_validate" => context as ValidateStoryCommand,
            "story_autofix" => context as AutoFixStoryJsonCommand,
            "story_summarize" => context as SummarizeStoryCommand,
            _ => null
        };
    }

    public async Task<string?> DetectPrimaryInstructionTypeAsync(string userQuery, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(userQuery))
        {
            _logger.LogWarning("DetectPrimaryInstructionTypeAsync called with empty query");
            return null;
        }

        var classification = await _intentClassification.ClassifyIntentAsync(userQuery, cancellationToken);
        if (classification == null || classification.InstructionTypes.Length == 0)
        {
            _logger.LogWarning("Intent classification returned no instruction types for query: {Query}", userQuery);
            return null;
        }

        return classification.InstructionTypes[0];
    }
}
