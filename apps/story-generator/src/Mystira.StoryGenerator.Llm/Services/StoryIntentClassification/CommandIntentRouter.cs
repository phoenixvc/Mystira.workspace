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

        // Heuristics for common commands
        var lowerQuery = userQuery.ToLowerInvariant().Trim();
        if (lowerQuery == "fix schema" || lowerQuery.Contains("auto-fix") || lowerQuery.Contains("autofix") || lowerQuery == "fix issues")
        {
            return "story_autofix";
        }
        if (lowerQuery == "validate" || lowerQuery == "check schema" || lowerQuery == "verify")
        {
            return "story_validate";
        }
        if (lowerQuery.StartsWith("summarize") || lowerQuery == "summary")
        {
            return "story_summarize";
        }
        if (lowerQuery.StartsWith("refine") || lowerQuery.StartsWith("update story") || lowerQuery.StartsWith("tweak story"))
        {
            return "story_generate_refine";
        }
        if (lowerQuery.StartsWith("generate") || lowerQuery.StartsWith("create story") || lowerQuery.StartsWith("make a story") || lowerQuery.StartsWith("write a story"))
        {
            return "story_generate_initial";
        }
        if (lowerQuery == "help" || lowerQuery == "what can you do")
        {
            return "help";
        }
        if (lowerQuery == "continue")
        {
            return "story_generate_initial"; // Or should this be handled specially?
                                            // ChatOrchestrationService has manual check for "continue".
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
