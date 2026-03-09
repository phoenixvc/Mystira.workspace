using System.Text;
using Microsoft.Extensions.Logging;
using Mystira.StoryGenerator.Contracts.Chat;
using Mystira.StoryGenerator.Domain.Commands.Chat;
using Mystira.StoryGenerator.Domain.Services;

namespace Mystira.StoryGenerator.Application.Handlers.Chat;

public static class FreeTextCommandHandler
{
    public static async Task<ChatCompletionResponse> Handle(
        FreeTextCommand command,
        ILlmServiceFactory llmFactory,
        IInstructionBlockService instructionBlockService,
        ILlmIntentLlmClassificationService llmIntentLlmClassificationService,
        ILogger logger,
        CancellationToken cancellationToken)
    {
        try
        {
            var context = command.Context ?? throw new InvalidOperationException("Chat context is required");
            var service = ResolveService(context, llmFactory);
            if (service is null)
            {
                return Failure("No LLM services are currently available");
            }

            var messages = command.History != null
                ? new List<MystiraChatMessage>(command.History)
                : new List<MystiraChatMessage>();

            var systemPrompt = BuildSystemPrompt(command.Intent, context.SystemPrompt);
            var instructionBlock = await ResolveInstructionBlockAsync(context, command.Intent, instructionBlockService, llmIntentLlmClassificationService, logger, cancellationToken);
            if (!string.IsNullOrWhiteSpace(instructionBlock))
            {
                messages.Insert(0, new MystiraChatMessage
                {
                    MessageType = ChatMessageType.System,
                    Content = instructionBlock,
                    Timestamp = DateTime.UtcNow
                });
            }

            var request = new ChatCompletionRequest
            {
                Provider = context.Provider,
                ModelId = context.ModelId,
                Model = context.Model,
                Messages = messages,
                Temperature = context.Temperature,
                MaxTokens = context.MaxTokens > 0 ? context.MaxTokens : 800,
                SystemPrompt = systemPrompt,
                JsonSchemaFormat = context.JsonSchemaFormat,
                IsSchemaValidationStrict = context.IsSchemaValidationStrict
            };

            return await service.CompleteAsync(request, cancellationToken);
        }
        catch (OperationCanceledException)
        {
            return Failure("Free-form request was cancelled.");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error handling free-text request");
            return Failure("An unexpected error occurred while generating the response.");
        }
    }

    private static ILLMService? ResolveService(ChatContext context, ILlmServiceFactory llmFactory)
    {
        return !string.IsNullOrWhiteSpace(context.Provider)
            ? llmFactory.GetService(context.Provider!)
            : llmFactory.GetDefaultService();
    }

    private static async Task<string?> ResolveInstructionBlockAsync(
        ChatContext context,
        string? intent,
        IInstructionBlockService instructionBlockService,
        ILlmIntentLlmClassificationService llmIntentLlmClassificationService,
        ILogger logger,
        CancellationToken cancellationToken)
    {
        if (context.Messages == null || context.Messages.Count == 0)
        {
            return null;
        }

        var userMessages = context.Messages
            .Where(message => message.MessageType == ChatMessageType.User)
            .TakeLast(4)
            .ToList();

        if (userMessages.Count == 0)
        {
            return null;
        }

        var builder = new StringBuilder();
        foreach (var message in userMessages)
        {
            builder.AppendLine(message.Content);
        }

        if (!string.IsNullOrWhiteSpace(context.SystemPrompt))
        {
            builder.AppendLine("SystemPrompt: " + context.SystemPrompt);
        }

        var queryText = builder.ToString();
        var categories = new[] { "story_generation", "meta" };
        var instructionTypes = !string.IsNullOrWhiteSpace(intent)
            ? new[] { intent! }
            : new[] { "guidelines" };
        var ageGroup = context?.CurrentStory?.AgeGroup ?? ExtractAgeGroupFromContext(context);

        var classification = await llmIntentLlmClassificationService.ClassifyAsync(queryText, cancellationToken);
        if (classification != null)
        {
            logger.LogInformation("Intent classified for free-text handler: {Categories} / {Types}", classification.Categories, classification.InstructionTypes);
            categories = classification.Categories;
            instructionTypes = classification.InstructionTypes;
        }

        var searchContext = new InstructionSearchContext
        {
            QueryText = queryText,
            Categories = categories,
            InstructionTypes = instructionTypes,
            TopK = 8,
            AgeGroup = ageGroup
        };

        return await instructionBlockService.BuildInstructionBlockAsync(searchContext, cancellationToken);
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

    private static string BuildSystemPrompt(string? intent, string? existingPrompt)
    {
        var builder = new StringBuilder();
        builder.AppendLine("You are Mystira's collaborative guide. Analyze the conversation and offer the most helpful next step.");
        builder.AppendLine("Blend storytelling expertise, schema awareness, and safety policy reminders when relevant.");
        builder.AppendLine("When appropriate, suggest concrete options (e.g., generate, refine, validate, request more info).");

        if (!string.IsNullOrWhiteSpace(intent))
        {
            builder.AppendLine();
            builder.AppendLine($"Primary intent detected: {intent}.");
        }

        if (!string.IsNullOrWhiteSpace(existingPrompt))
        {
            builder.AppendLine();
            builder.AppendLine("Additional system guidance:");
            builder.AppendLine(existingPrompt);
        }

        return builder.ToString().Trim();
    }

    private static ChatCompletionResponse Failure(string message)
    {
        return new ChatCompletionResponse
        {
            Success = false,
            Error = message,
            Provider = "system",
            Model = "free-text"
        };
    }
}
