using Microsoft.Extensions.Logging;
using Mystira.StoryGenerator.Domain.Commands.Stories;

namespace Mystira.StoryGenerator.Llm.Services.Intent;

public interface ICommandIntentRouter
{
    Task<object?> RouteIntentToCommandAsync(string userQuery, object? context = null, CancellationToken cancellationToken = default);
}

public class CommandIntentRouter : ICommandIntentRouter
{
    private readonly Mystira.StoryGenerator.Domain.Services.IIntentRouterService _intentRouter;
    private readonly ILogger<CommandIntentRouter> _logger;

    public CommandIntentRouter(
        Mystira.StoryGenerator.Domain.Services.IIntentRouterService intentRouter,
        ILogger<CommandIntentRouter> logger)
    {
        _intentRouter = intentRouter;
        _logger = logger;
    }

    public async Task<object?> RouteIntentToCommandAsync(string userQuery, object? context = null, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(userQuery))
        {
            _logger.LogWarning("RouteIntentToCommandAsync called with empty query");
            return null;
        }

        var classification = await _intentRouter.ClassifyIntentAsync(userQuery, cancellationToken);
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
}
