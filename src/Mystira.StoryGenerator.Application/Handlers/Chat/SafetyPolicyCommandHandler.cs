using System.Text;
using Microsoft.Extensions.Logging;
using Mystira.StoryGenerator.Contracts.Chat;
using Mystira.StoryGenerator.Domain.Commands;
using Mystira.StoryGenerator.Domain.Commands.Chat;
using Mystira.StoryGenerator.Domain.Services;

namespace Mystira.StoryGenerator.Application.Handlers.Chat;

public class SafetyPolicyCommandHandler : ICommandHandler<SafetyPolicyCommand, ChatCompletionResponse>
{
    private const string SafetyBaseline = @"Mystira Safety & Content Baseline:
- Stories must remain emotionally safe for the declared age group.
- No profanity, slurs, sexual content, self-harm, or graphic violence.
- Mild peril is acceptable but must resolve through empathy, cooperation, or creative problem solving.
- Encourage growth mindset: mistakes are learning opportunities and characters can repair harm.
- Highlight fairness, kindness, accountability, and inclusion; never punch down at any group.
- Final scenes should leave the player encouraged, supported, and empowered.";

    private readonly ILLMServiceFactory _llmFactory;
    private readonly IInstructionBlockService _instructionBlockService;
    private readonly ILogger<SafetyPolicyCommandHandler> _logger;

    public SafetyPolicyCommandHandler(
        ILLMServiceFactory llmFactory,
        IInstructionBlockService instructionBlockService,
        ILogger<SafetyPolicyCommandHandler> logger)
    {
        _llmFactory = llmFactory;
        _instructionBlockService = instructionBlockService;
        _logger = logger;
    }

    public async Task<ChatCompletionResponse> Handle(SafetyPolicyCommand command, CancellationToken cancellationToken)
    {
        try
        {
            var context = command.Context ?? throw new InvalidOperationException("Chat context is required");
            var service = ResolveService(context);
            if (service is null)
            {
                return Failure("No LLM services are currently available for safety guidance.");
            }

            var ragBlock = await ResolveInstructionBlockAsync(command.UserQuery, cancellationToken);
            var contextSummary = ChatContextSummaryBuilder.BuildContextSummary(context);
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

            messages.Add(new MystiraChatMessage
            {
                MessageType = ChatMessageType.System,
                Content = SafetyBaseline,
                Timestamp = DateTime.UtcNow
            });

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
                Content = BuildSafetyPrompt(command.UserQuery),
                Timestamp = DateTime.UtcNow
            });

            var maxTokens = Math.Max(700, context.MaxTokens);
            var request = new ChatCompletionRequest
            {
                Provider = context.Provider ?? service.ProviderName,
                ModelId = context.ModelId,
                Model = context.Model,
                Temperature = 0.2,
                MaxTokens = maxTokens,
                Messages = messages,
                SystemPrompt = BuildSystemPrompt()
            };

            return await service.CompleteAsync(request, cancellationToken);
        }
        catch (OperationCanceledException)
        {
            return Failure("Safety policy request was cancelled.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error answering safety policy question");
            return Failure("An unexpected error occurred while answering safety questions.");
        }
    }

    private ILLMService? ResolveService(ChatContext context)
    {
        return !string.IsNullOrWhiteSpace(context.Provider)
            ? _llmFactory.GetService(context.Provider!)
            : _llmFactory.GetDefaultService();
    }

    private async Task<string?> ResolveInstructionBlockAsync(string? userQuery, CancellationToken cancellationToken)
    {
        var query = string.IsNullOrWhiteSpace(userQuery)
            ? "Mystira safety policy overview"
            : userQuery!;

        var searchContext = new InstructionSearchContext
        {
            QueryText = query,
            Categories = new[] { "safety", "story_generation" },
            InstructionTypes = new[] { "safety_policy" },
            TopK = 6
        };

        return await _instructionBlockService.BuildInstructionBlockAsync(searchContext, cancellationToken);
    }

    private static string BuildSafetyPrompt(string? userQuery)
    {
        if (string.IsNullOrWhiteSpace(userQuery))
        {
            return "Summarize the Mystira safety policy for child-friendly branching stories.";
        }

        var builder = new StringBuilder();
        builder.AppendLine("Answer this safety question in detail while referencing policy bullet points:");
        builder.AppendLine(userQuery.Trim());
        builder.AppendLine();
        builder.AppendLine("Include allowed content, disallowed content, remediation steps, and concrete guardrails.");
        return builder.ToString();
    }

    private static string BuildSystemPrompt()
    {
        return @"You are Mystira's safety and wellbeing advisor.
Ground every answer in the official child-safety policy plus any retrieved documentation.
Always cover:
1. Age window and rationale
2. Allowed vs. disallowed content (with examples)
3. How to revise stories to stay compliant.
If information is missing, clearly say so and provide best-effort guidance.";
    }

    private static ChatCompletionResponse Failure(string message)
    {
        return new ChatCompletionResponse
        {
            Success = false,
            Error = message,
            Provider = "system",
            Model = "safety-policy"
        };
    }
}
