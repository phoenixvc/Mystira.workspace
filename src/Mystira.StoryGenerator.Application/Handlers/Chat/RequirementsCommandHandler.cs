using System.Text;
using Microsoft.Extensions.Logging;
using Mystira.StoryGenerator.Contracts.Chat;
using Mystira.StoryGenerator.Domain.Commands;
using Mystira.StoryGenerator.Domain.Commands.Chat;
using Mystira.StoryGenerator.Domain.Services;

namespace Mystira.StoryGenerator.Application.Handlers.Chat;

public class RequirementsCommandHandler : ICommandHandler<RequirementsCommand, ChatCompletionResponse>
{
    private readonly ILLMServiceFactory _llmFactory;
    private readonly IInstructionBlockService _instructionBlockService;
    private readonly ILogger<RequirementsCommandHandler> _logger;

    public RequirementsCommandHandler(
        ILLMServiceFactory llmFactory,
        IInstructionBlockService instructionBlockService,
        ILogger<RequirementsCommandHandler> logger)
    {
        _llmFactory = llmFactory;
        _instructionBlockService = instructionBlockService;
        _logger = logger;
    }

    public async Task<ChatCompletionResponse> Handle(RequirementsCommand command, CancellationToken cancellationToken)
    {
        try
        {
            var context = command.Context ?? throw new InvalidOperationException("Chat context is required");
            var service = ResolveService(context);
            if (service is null)
            {
                return Failure("No LLM services are currently available for requirements questions.");
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
                Content = BuildRequirementsPrompt(command.UserQuery),
                Timestamp = DateTime.UtcNow
            });

            var maxTokens = Math.Max(800, context.MaxTokens);
            var request = new ChatCompletionRequest
            {
                Provider = context.Provider ?? service.ProviderName,
                ModelId = context.ModelId,
                Model = context.Model,
                Temperature = 0.25,
                MaxTokens = maxTokens,
                Messages = messages,
                SystemPrompt = BuildSystemPrompt()
            };

            return await service.CompleteAsync(request, cancellationToken);
        }
        catch (OperationCanceledException)
        {
            return Failure("Requirements request was cancelled.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error answering requirements question");
            return Failure("An unexpected error occurred while answering requirements.");
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

        var queryText = builder.Length > 0 ? builder.ToString() : "Mystira story requirements overview";
        var ageGroup = context?.CurrentStory?.AgeGroup ?? ExtractAgeGroupFromContext(context);

        var searchContext = new InstructionSearchContext
        {
            QueryText = queryText,
            Categories = new[] { "story_generation" },
            InstructionTypes = new[] { "requirements" },
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

    private static string BuildRequirementsPrompt(string? userQuery)
    {
        if (string.IsNullOrWhiteSpace(userQuery))
        {
            return "List the non-negotiable requirements for a valid Mystira branching story output.";
        }

        var builder = new StringBuilder();
        builder.AppendLine("Summarize the requirements requested below, tailoring them to the current session context:");
        builder.AppendLine(userQuery.Trim());
        builder.AppendLine();
        builder.AppendLine("Break responses into sections such as schema, branching, characters, developmental goals, and safety.");
        return builder.ToString();
    }

    private static string BuildSystemPrompt()
    {
        return @"You are Mystira's requirements auditor.
Respond with concrete checklists covering:
- JSON/schema compliance (required keys, data types, counts)
- Story structure (scene counts, branching rules, endings)
- Character expectations and metadata
- Developmental or educational outcomes.
Whenever possible, cite the rule or dataset that enforces the requirement.";
    }

    private static ChatCompletionResponse Failure(string message)
    {
        return new ChatCompletionResponse
        {
            Success = false,
            Error = message,
            Provider = "system",
            Model = "requirements"
        };
    }
}
