using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Mystira.StoryGenerator.Contracts.Chat;
using Mystira.StoryGenerator.Contracts.Configuration;
using Mystira.StoryGenerator.Contracts.Stories;
using Mystira.StoryGenerator.Domain.Commands.Stories;
using Mystira.StoryGenerator.Domain.Services;
using Mystira.StoryGenerator.Application.Utilities;

namespace Mystira.StoryGenerator.Application.Handlers.Stories;

public static class AutoFixStoryJsonCommandHandler
{
    public static async Task<GenerateJsonStoryResponse> Handle(
        AutoFixStoryJsonCommand command,
        ILlmServiceFactory llmFactory,
        IOptions<AiSettings> aiOptions,
        IStorySchemaProvider schemaProvider,
        ILogger logger,
        CancellationToken cancellationToken)
    {
        var settings = aiOptions.Value;

        try
        {
            var service = !string.IsNullOrWhiteSpace(command.Provider)
                ? llmFactory.GetService(command.Provider!, command.Model)
                : llmFactory.GetDefaultService();

            if (service is null)
            {
                return new GenerateJsonStoryResponse
                {
                    Success = false,
                    Error = "No LLM services are currently available"
                };
            }

            var resolvedModelId = string.IsNullOrWhiteSpace(command.Provider) ? null : command.Provider;
            var resolvedModelName = string.IsNullOrWhiteSpace(command.Model) ? null : command.Model;
            var temperature = Math.Min(0.3, settings.DefaultTemperature);
            var maxTokens = Math.Max(2000, settings.DefaultMaxTokens);

            var systemPrompt = BuildAutoFixSystemPrompt();
            var messages = BuildAutoFixMessages(command.StoryJson);

            var chatRequest = new ChatCompletionRequest
            {
                Provider = service.ProviderName,
                Model = resolvedModelName,
                Temperature = temperature,
                MaxTokens = maxTokens,
                Messages = messages,
                SystemPrompt = systemPrompt,
                JsonSchemaFormat = await LoadSchemaFormatSafeAsync(schemaProvider)
            };

            var response = await service.CompleteAsync(chatRequest, cancellationToken);
            if (!response.Success)
            {
                return new GenerateJsonStoryResponse
                {
                    Success = false,
                    Error = response.Error ?? "LLM returned an error",
                    Provider = response.Provider ?? service.ProviderName,
                    Model = response.Model ?? resolvedModelName ?? string.Empty,
                    ModelId = response.ModelId ?? resolvedModelId
                };
            }

            return new GenerateJsonStoryResponse
            {
                Success = true,
                Json = StoryTextSanitizer.CollapseNewlinesToSpace(response.Content) ?? string.Empty,
                Provider = response.Provider ?? service.ProviderName,
                Model = response.Model ?? resolvedModelName ?? string.Empty,
                ModelId = response.ModelId ?? resolvedModelId
            };
        }
        catch (OperationCanceledException)
        {
            return new GenerateJsonStoryResponse
            {
                Success = false,
                Error = "Request was cancelled"
            };
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error auto-fixing story JSON");
            return new GenerateJsonStoryResponse
            {
                Success = false,
                Error = "An unexpected error occurred during auto-fix"
            };
        }
    }

    private static string BuildAutoFixSystemPrompt()
    {
        return @"
You are a JSON validation and repair specialist for branching adventure stories. Your job is to analyze malformed or incomplete story JSON and repair it.

When fixing a story:
- Identify and fix JSON syntax errors (missing quotes, commas, brackets, etc.)
- Ensure all required fields are present and properly formatted
- Fix type mismatches (e.g., ensure numbers are not quoted strings when they should be integers)
- Ensure all IDs are lowercase snake_case
- Validate that scene references (next_scene, branches) point to valid scene IDs
- Return only the corrected JSON with no commentary or code fences
- If the JSON is irreparable, return a minimal valid structure with an error note

Output must be a single valid JSON object.
";
    }

    private static List<MystiraChatMessage> BuildAutoFixMessages(string brokenJson)
    {
        var messages = new List<MystiraChatMessage>
        {
            new MystiraChatMessage
            {
                MessageType = ChatMessageType.User,
                Content = $"Please fix this broken story JSON:\n\n{brokenJson}",
                Timestamp = DateTime.UtcNow
            }
        };

        return messages;
    }

    private static async Task<JsonSchemaResponseFormat?> LoadSchemaFormatSafeAsync(IStorySchemaProvider schemaProvider)
    {
        try
        {
            var json = await schemaProvider.GetSchemaJsonAsync();
            if (!string.IsNullOrWhiteSpace(json))
            {
                return new JsonSchemaResponseFormat
                {
                    FormatName = "mystira-story-fixed",
                    SchemaJson = json,
                    IsStrict = schemaProvider.IsStrict
                };
            }
        }
        catch
        {
            // ignore and fall back
        }
        return null;
    }
}
