using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Mystira.StoryGenerator.Contracts.Chat;
using Mystira.StoryGenerator.Contracts.Configuration;
using Mystira.StoryGenerator.Contracts.Stories;
using Mystira.StoryGenerator.Domain.Commands;
using Mystira.StoryGenerator.Domain.Commands.Stories;
using Mystira.StoryGenerator.Domain.Services;

namespace Mystira.StoryGenerator.Application.Handlers.Stories;

public class RefineStoryCommandHandler : ICommandHandler<RefineStoryCommand, GenerateJsonStoryResponse>
{
    private readonly ILLMServiceFactory _llmFactory;
    private readonly AiSettings _settings;
    private readonly IStorySchemaProvider _schemaProvider;
    private readonly ILogger<RefineStoryCommandHandler> _logger;

    public RefineStoryCommandHandler(
        ILLMServiceFactory llmFactory,
        IOptions<AiSettings> aiOptions,
        IStorySchemaProvider schemaProvider,
        ILogger<RefineStoryCommandHandler> logger)
    {
        _llmFactory = llmFactory;
        _settings = aiOptions.Value;
        _schemaProvider = schemaProvider;
        _logger = logger;
    }

    public async Task<GenerateJsonStoryResponse> Handle(RefineStoryCommand command, CancellationToken cancellationToken)
    {
        try
        {
            var request = command.Request;
            var service = !string.IsNullOrWhiteSpace(request.Provider)
                ? _llmFactory.GetService(request.Provider!)
                : _llmFactory.GetDefaultService();

            if (service is null)
            {
                return new GenerateJsonStoryResponse
                {
                    Success = false,
                    Error = "No LLM services are currently available"
                };
            }

            var resolvedModelId = string.IsNullOrWhiteSpace(request.ModelId) ? null : request.ModelId;
            var resolvedModelName = string.IsNullOrWhiteSpace(request.Model) ? null : request.Model;
            var temperature = _settings.DefaultTemperature;
            var maxTokens = Math.Max(1200, _settings.DefaultMaxTokens);

            var systemPrompt = BuildRefinementSystemPrompt();
            var messages = BuildRefinementMessages(command.RefinementPrompt, command.CurrentStory);

            var chatRequest = new ChatCompletionRequest
            {
                Provider = service.ProviderName,
                ModelId = resolvedModelId,
                Model = resolvedModelName,
                Temperature = temperature,
                MaxTokens = maxTokens,
                Messages = messages,
                SystemPrompt = systemPrompt,
                JsonSchemaFormat = LoadSchemaFormatSafe()
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
                Json = response.Content ?? string.Empty,
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
            _logger.LogError(ex, "Error refining story");
            return new GenerateJsonStoryResponse
            {
                Success = false,
                Error = "An unexpected error occurred during refinement"
            };
        }
    }

    private static string BuildRefinementSystemPrompt()
    {
        return @"
You are a professional interactive storytelling refinement engine. Your job is to take an existing branching adventure story
and refine it based on user feedback while maintaining the JSON structure and all required fields.

When refining a story:
- Keep the overall structure intact (title, characters, scenes, metadata)
- Make targeted changes only to the sections requested
- Ensure all IDs remain lowercase snake_case
- Validate that all required fields are present in the output
- Return only valid JSON with no commentary or code fences

Output must be a single valid JSON object.
";
    }

    private static List<MystiraChatMessage> BuildRefinementMessages(string refinementPrompt,
        StorySnapshot? commandCurrentStory)
    {
        var messages = new List<MystiraChatMessage>
        {
            new MystiraChatMessage
            {
                MessageType = ChatMessageType.User,
                Content = "Current JSON story:\n" + commandCurrentStory?.Content,
                Timestamp = DateTime.UtcNow
            },
            new MystiraChatMessage
            {
                MessageType = ChatMessageType.User,
                Content = refinementPrompt,
                Timestamp = DateTime.UtcNow
            }
        };

        return messages;
    }

    private JsonSchemaResponseFormat? LoadSchemaFormatSafe()
    {
        try
        {
            var json = _schemaProvider.GetSchemaJsonAsync().Result;
            if (!string.IsNullOrWhiteSpace(json))
            {
                return new JsonSchemaResponseFormat
                {
                    FormatName = "mystira-story-refined",
                    SchemaJson = json,
                    IsStrict = _schemaProvider.IsStrict
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
