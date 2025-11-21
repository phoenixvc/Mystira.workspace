using System.Text.Json;
using Microsoft.Extensions.Options;
using Mystira.StoryGenerator.Api.Services.LLM;
using Mystira.StoryGenerator.Contracts.Chat;
using Mystira.StoryGenerator.Contracts.Configuration;
using Mystira.StoryGenerator.Contracts.Intent;

namespace Mystira.StoryGenerator.Api.Services.Intent;

public class IntentRouterService : IIntentRouterService
{
    private readonly IntentRouterSettings _settings;
    private readonly ILLMServiceFactory _llmServiceFactory;
    private readonly ILogger<IntentRouterService> _logger;

    private const string ClassificationPrompt = @"
You are the Mystira RAG intent classifier.
Given a single user instruction, map it to:

categories (one or more of: story_generation | validation | autofix | summarization | config | safety | meta)

instructionTypes (one or more of: story_generate_initial | story_generate_refine | story_validate | story_autofix | story_summarize | config_view | config_update | help | schema_docs | safety_policy | requirements | guidelines)

Most of the time, a single category and a single instructionType is enough.
Only return multiple values if the instruction clearly spans multiple operations
(e.g., “generate a story and then summarize it”).

Return ONLY a JSON object with the following format, no additional text:
{
  ""categories"": [""<category1>"", ""<category2>""],
  ""instructionTypes"": [""<instructionType1>"", ""<instructionType2>""]
}

Rules:
- ""categories"" MUST contain at least one allowed value.
- ""instructionTypes"" MUST contain at least one allowed value.
- If the intent is clearly focused on one operation, return arrays of length 1.
- If unsure between two closely related options, you MAY include both.

Examples:
User: ""Create a new story about a knight""
Response: {""categories"": [""story_generation""], ""instructionTypes"": [""story_generate_initial""]}

User: ""Validate my JSON""
Response: {""categories"": [""validation""], ""instructionTypes"": [""story_validate""]}

User: ""What are the requirements for story generation?""
Response: {""categories"": [""story_generation""], ""instructionTypes"": [""requirements""]}

User: ""Show me the config""
Response: {""categories"": [""config""], ""instructionTypes"": [""config_view""]}

User: ""Generate a new story and then summarize it""
Response: {""categories"": [""story_generation"", ""summarization""], ""instructionTypes"": [""story_generate_initial"", ""story_summarize""]}

Now classify this user instruction:
";

    public IntentRouterService(
        IOptions<AiSettings> aiOptions,
        ILLMServiceFactory llmServiceFactory,
        ILogger<IntentRouterService> logger)
    {
        _settings = aiOptions.Value.IntentRouter;
        _llmServiceFactory = llmServiceFactory;
        _logger = logger;
    }

    public async Task<IntentClassification?> ClassifyIntentAsync(string userQuery, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(userQuery))
        {
            _logger.LogWarning("Intent classification requested with empty query");
            return null;
        }

        if (!_settings.IsConfigured)
        {
            _logger.LogDebug("Intent router is not configured, skipping classification");
            return null;
        }

        try
        {
            var llmService = _llmServiceFactory.GetService(_settings.Provider!);
            if (llmService == null)
            {
                _logger.LogWarning("LLM provider {Provider} not available for intent routing", _settings.Provider);
                return null;
            }

            var request = new ChatCompletionRequest
            {
                Provider = _settings.Provider,
                ModelId = _settings.ModelId,
                Model = _settings.ModelId,
                Temperature = _settings.Temperature,
                MaxTokens = _settings.MaxTokens,
                Messages = new List<MystiraChatMessage>
                {
                    new MystiraChatMessage
                    {
                        MessageType = ChatMessageType.System,
                        Content = ClassificationPrompt,
                        Timestamp = DateTime.UtcNow
                    },
                    new MystiraChatMessage
                    {
                        MessageType = ChatMessageType.User,
                        Content = userQuery,
                        Timestamp = DateTime.UtcNow
                    }
                }
            };

            _logger.LogInformation("Calling intent router with provider {Provider}, model {Model}",
                _settings.Provider, _settings.ModelId);

            var response = await llmService.CompleteAsync(request, cancellationToken);

            if (!response.Success || string.IsNullOrWhiteSpace(response.Content))
            {
                _logger.LogWarning("Intent classification failed: {Error}", response.Error);
                return null;
            }

            _logger.LogDebug("Intent router response: {Content}", response.Content);

            return ParseClassification(response.Content);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during intent classification");
            return null;
        }
    }

    private IntentClassification? ParseClassification(string jsonContent)
    {
        try
        {
            var cleanedJson = ExtractJsonFromResponse(jsonContent);

            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                AllowTrailingCommas = true
            };

            var classification = JsonSerializer.Deserialize<IntentClassification>(cleanedJson, options);

            if (classification == null ||
                classification.Categories.Length == 0 ||
                classification.Categories[0] == string.Empty ||
                classification.InstructionTypes.Length == 0 ||
                classification.InstructionTypes[0] == string.Empty)
            {
                _logger.LogWarning("Parsed classification is incomplete");
                return null;
            }

            _logger.LogInformation("Successfully classified intent: category={Category}, instructionType={InstructionType}",
                classification.Categories, classification.InstructionTypes);

            return classification;
        }
        catch (JsonException ex)
        {
            _logger.LogError(ex, "Failed to parse intent classification JSON: {Content}", jsonContent);
            return null;
        }
    }

    private static string ExtractJsonFromResponse(string content)
    {
        var trimmed = content.Trim();

        var jsonStart = trimmed.IndexOf('{');
        var jsonEnd = trimmed.LastIndexOf('}');

        if (jsonStart >= 0 && jsonEnd > jsonStart)
        {
            return trimmed.Substring(jsonStart, jsonEnd - jsonStart + 1);
        }

        return trimmed;
    }
}
