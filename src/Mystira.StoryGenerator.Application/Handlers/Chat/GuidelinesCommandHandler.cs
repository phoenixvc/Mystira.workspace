using System.Text;
using Microsoft.Extensions.Logging;
using Mystira.StoryGenerator.Contracts.Chat;
using Mystira.StoryGenerator.Domain.Commands;
using Mystira.StoryGenerator.Domain.Commands.Chat;
using Mystira.StoryGenerator.Domain.Services;

namespace Mystira.StoryGenerator.Application.Handlers.Chat;

public class GuidelinesCommandHandler : ICommandHandler<GuidelinesCommand, ChatCompletionResponse>
{
    private readonly ILLMServiceFactory _llmFactory;
    private readonly IInstructionBlockService _instructionBlockService;
    private readonly ILogger<GuidelinesCommandHandler> _logger;

    public GuidelinesCommandHandler(
        ILLMServiceFactory llmFactory,
        IInstructionBlockService instructionBlockService,
        ILogger<GuidelinesCommandHandler> logger)
    {
        _llmFactory = llmFactory;
        _instructionBlockService = instructionBlockService;
        _logger = logger;
    }

    public async Task<ChatCompletionResponse> Handle(GuidelinesCommand command, CancellationToken cancellationToken)
    {
        try
        {
            var context = command.Context ?? throw new InvalidOperationException("Chat context is required");
            var service = ResolveService(context);
            if (service is null)
            {
                return Failure("No LLM services are currently available for guidelines.");
            }

            var contextSummary = ChatContextSummaryBuilder.BuildContextSummary(context);
            var ragBlock = await ResolveInstructionBlockAsync(command.UserQuery, contextSummary, context, cancellationToken);
            var messages = new List<MystiraChatMessage>();

            if (!string.IsNullOrWhiteSpace(ragBlock))
            {
                messages.Add(new MystiraChatMessage
                {
                    MessageType = ChatMessageType.System,
                    Content = ragBlock,
                    Timestamp = DateTime.UtcNow
                });
            }

            if (!string.IsNullOrWhiteSpace(contextSummary))
            {
                messages.Add(new MystiraChatMessage
                {
                    MessageType = ChatMessageType.System,
                    Content = "Session context:\n" + contextSummary,
                    Timestamp = DateTime.UtcNow
                });
            }

            messages.Add(new MystiraChatMessage
            {
                MessageType = ChatMessageType.User,
                Content = BuildGuidelinesPrompt(command.UserQuery),
                Timestamp = DateTime.UtcNow
            });

            var maxTokens = Math.Max(900, context.MaxTokens);
            var request = new ChatCompletionRequest
            {
                Provider = context.Provider ?? service.ProviderName,
                ModelId = context.ModelId,
                Model = context.Model,
                Temperature = 0.35,
                MaxTokens = maxTokens,
                Messages = messages,
                SystemPrompt = BuildSystemPrompt()
            };

            return await service.CompleteAsync(request, cancellationToken);
        }
        catch (OperationCanceledException)
        {
            return Failure("Guidelines request was cancelled.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error answering guidelines question");
            return Failure("An unexpected error occurred while sharing guidelines.");
        }
    }

    private ILLMService? ResolveService(ChatContext context)
    {
        return !string.IsNullOrWhiteSpace(context.Provider)
            ? _llmFactory.GetService(context.Provider!)
            : _llmFactory.GetDefaultService();
    }

    private async Task<string?> ResolveInstructionBlockAsync(string? userQuery, string contextSummary, ChatContext context, CancellationToken cancellationToken)
    {
        var builder = new StringBuilder();
        if (!string.IsNullOrWhiteSpace(userQuery))
        {
            builder.AppendLine(userQuery.Trim());
        }

        if (!string.IsNullOrWhiteSpace(contextSummary))
        {
            builder.AppendLine(contextSummary);
        }

        var queryText = builder.Length > 0 ? builder.ToString() : "Mystira story writing guidelines";
        var ageGroup = context?.CurrentStory?.AgeGroup ?? ExtractAgeGroupFromContext(context);

        var searchContext = new InstructionSearchContext
        {
            QueryText = queryText,
            Categories = new[] { "story_generation" },
            InstructionTypes = new[] { "guidelines" },
            TopK = 6,
            AgeGroup = ageGroup
        };

        return await _instructionBlockService.BuildInstructionBlockAsync(searchContext, cancellationToken);
    }

    private static string? ExtractAgeGroupFromContext(ChatContext? context)
    {
        if (context?.CurrentStory == null)
            return null;
        
        return ExtractAgeGroupFromJson(context.CurrentStory.Content);
    }

    private static string? ExtractAgeGroupFromJson(string? jsonContent)
    {
        if (string.IsNullOrWhiteSpace(jsonContent))
            return null;

        try
        {
            var json = System.Text.Json.JsonDocument.Parse(jsonContent);
            if (json.RootElement.TryGetProperty("age_group", out var ageGroupElement))
            {
                return ageGroupElement.GetString();
            }
        }
        catch
        {
            // If parsing fails, return null and let the system use default index
        }

        return null;
    }

    private static string BuildGuidelinesPrompt(string? userQuery)
    {
        if (string.IsNullOrWhiteSpace(userQuery))
        {
            return "Share practical guidelines for crafting Mystira branching stories.";
        }

        var builder = new StringBuilder();
        builder.AppendLine("Provide creative guidelines tailored to the request below:");
        builder.AppendLine(userQuery.Trim());
        builder.AppendLine();
        builder.AppendLine("Offer actionable suggestions (structure, pacing, emotional beats, next steps). Include sample prompts or questions we can ask the players.");
        return builder.ToString();
    }

    private static string BuildSystemPrompt()
    {
        return @"You are Mystira's creative guide.
Deliver guidelines that are:
- Grounded in the official requirements and safety policy
- Action oriented (bullet lists, next steps, sample beats)
- Context aware (reference the current story or goals when known)
Suggest concrete actions the author can take next.";
    }

    private static ChatCompletionResponse Failure(string message)
    {
        return new ChatCompletionResponse
        {
            Success = false,
            Error = message,
            Provider = "system",
            Model = "guidelines"
        };
    }
}
