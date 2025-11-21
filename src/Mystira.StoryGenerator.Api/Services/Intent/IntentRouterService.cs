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

    private const string ClassificationPrompt = @"You are the Mystira RAG intent classifier.
Given a single user instruction, map it to:

category (one of: story_generation | validation | autofix | summarization | config | safety | meta)

instructionType (one of: story_generate_initial | story_generate_refine | story_validate | story_autofix | story_summarize | config_view | config_update | help | schema_docs | safety_policy | requirements | guidelines)

Return ONLY a JSON object with the following format, no additional text:
{
  ""category"": ""<category>"",
  ""instructionType"": ""<instructionType>""
}

Examples:
User: ""Create a new story about a knight""
Response: {""category"": ""story_generation"", ""instructionType"": ""story_generate_initial""}

User: ""Validate my YAML""
Response: {""category"": ""validation"", ""instructionType"": ""story_validate""}

User: ""What are the requirements for story generation?""
Response: {""category"": ""story_generation"", ""instructionType"": ""requirements""}

User: ""Show me the config""
Response: {""category"": ""config"", ""instructionType"": ""config_view""}

Now classify this user instruction:";

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
                string.IsNullOrWhiteSpace(classification.Category) || 
                string.IsNullOrWhiteSpace(classification.InstructionType))
            {
                _logger.LogWarning("Parsed classification is incomplete");
                return null;
            }

            _logger.LogInformation("Successfully classified intent: category={Category}, instructionType={InstructionType}", 
                classification.Category, classification.InstructionType);

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
