using System.Text;
using Microsoft.Extensions.Logging;
using Mystira.StoryGenerator.Contracts.Chat;
using Mystira.StoryGenerator.Domain.Commands;
using Mystira.StoryGenerator.Domain.Commands.Chat;
using Mystira.StoryGenerator.Domain.Services;

namespace Mystira.StoryGenerator.Application.Handlers.Chat;

public class SchemaDocsCommandHandler : ICommandHandler<SchemaDocsCommand, ChatCompletionResponse>
{
    private const int SchemaSnippetLimit = 10000;

    private readonly ILLMServiceFactory _llmFactory;
    private readonly IInstructionBlockService _instructionBlockService;
    private readonly IStorySchemaProvider _schemaProvider;
    private readonly ILogger<SchemaDocsCommandHandler> _logger;

    public SchemaDocsCommandHandler(
        ILLMServiceFactory llmFactory,
        IInstructionBlockService instructionBlockService,
        IStorySchemaProvider schemaProvider,
        ILogger<SchemaDocsCommandHandler> logger)
    {
        _llmFactory = llmFactory;
        _instructionBlockService = instructionBlockService;
        _schemaProvider = schemaProvider;
        _logger = logger;
    }

    public async Task<ChatCompletionResponse> Handle(SchemaDocsCommand command, CancellationToken cancellationToken)
    {
        try
        {
            var context = command.Context ?? throw new InvalidOperationException("Chat context is required");
            var service = ResolveService(context);
            if (service is null)
            {
                return Failure("No LLM services are currently available for schema explanations.");
            }

            var schemaJson = await _schemaProvider.GetSchemaJsonAsync(cancellationToken);
            if (string.IsNullOrWhiteSpace(schemaJson))
            {
                return Failure("The Mystira story schema could not be loaded. Please verify the schema configuration.");
            }

            var instructionBlock = await ResolveInstructionBlockAsync(command.UserQuery, context, cancellationToken);
            var messages = new List<MystiraChatMessage>();

            if (!string.IsNullOrWhiteSpace(instructionBlock))
            {
                messages.Add(new MystiraChatMessage
                {
                    MessageType = ChatMessageType.System,
                    Content = instructionBlock,
                    Timestamp = DateTime.UtcNow
                });
            }

            messages.Add(new MystiraChatMessage
            {
                MessageType = ChatMessageType.System,
                Content = BuildSchemaExcerpt(schemaJson),
                Timestamp = DateTime.UtcNow
            });

            messages.Add(new MystiraChatMessage
            {
                MessageType = ChatMessageType.User,
                Content = BuildSchemaQuestion(command.UserQuery),
                Timestamp = DateTime.UtcNow
            });

            var maxTokens = Math.Max(900, context.MaxTokens);
            var chatRequest = new ChatCompletionRequest
            {
                Provider = context.Provider ?? service.ProviderName,
                ModelId = context.ModelId,
                Model = context.Model,
                Temperature = 0.15,
                MaxTokens = maxTokens,
                Messages = messages,
                SystemPrompt = BuildSystemPrompt()
            };

            var response = await service.CompleteAsync(chatRequest, cancellationToken);
            return response;
        }
        catch (OperationCanceledException)
        {
            return Failure("Schema documentation request was cancelled.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error answering schema documentation question");
            return Failure("An unexpected error occurred while answering schema questions.");
        }
    }

    private ILLMService? ResolveService(ChatContext context)
    {
        return !string.IsNullOrWhiteSpace(context.Provider)
            ? _llmFactory.GetService(context.Provider!)
            : _llmFactory.GetDefaultService();
    }

    private async Task<string?> ResolveInstructionBlockAsync(string? userQuery, ChatContext context, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(userQuery))
        {
            return null;
        }

        var ageGroup = context?.CurrentStory?.AgeGroup ?? ExtractAgeGroupFromContext(context);

        var searchContext = new InstructionSearchContext
        {
            QueryText = userQuery,
            Categories = new[] { "meta", "story_generation" },
            InstructionTypes = new[] { "schema_docs" },
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

    private static string BuildSchemaExcerpt(string schemaJson)
    {
        var snippet = schemaJson.Length > SchemaSnippetLimit
            ? schemaJson[..SchemaSnippetLimit] + "\n... (truncated; see config/story-schema.json for the complete schema)."
            : schemaJson;

        var builder = new StringBuilder();
        builder.AppendLine("Canonical Mystira story schema excerpt (source: config/story-schema.json):");
        builder.AppendLine(snippet);
        return builder.ToString().TrimEnd();
    }

    private static string BuildSchemaQuestion(string? userQuery)
    {
        if (string.IsNullOrWhiteSpace(userQuery))
        {
            return "Explain the major sections of the Mystira story JSON schema, including required fields and important enums.";
        }

        var builder = new StringBuilder();
        builder.AppendLine("Answer the following schema question using the excerpt and any retrieved docs:");
        builder.AppendLine(userQuery.Trim());
        builder.AppendLine();
        builder.AppendLine("Highlight required fields, data types, enum values, and structural rules. Include short JSON examples when helpful.");
        return builder.ToString();
    }

    private static string BuildSystemPrompt()
    {
        return @"You are Mystira's schema navigator. Use the canonical JSON schema excerpt plus any retrieved documentation to explain:
- Field names, descriptions, and data types
- Required vs. optional sections
- Allowed enum values, ranges, and constraints
- Relationships between scenes, characters, and metadata

Guidelines:
- Quote field names exactly as they appear in the schema
- Present answers in sections or bullet lists for readability
- Include compact JSON snippets when clarifying structure
- If the user asks about something the schema does not cover, state that clearly and suggest alternatives
";
    }

    private static ChatCompletionResponse Failure(string message)
    {
        return new ChatCompletionResponse
        {
            Success = false,
            Error = message,
            Provider = "system",
            Model = "schema-docs"
        };
    }
}
