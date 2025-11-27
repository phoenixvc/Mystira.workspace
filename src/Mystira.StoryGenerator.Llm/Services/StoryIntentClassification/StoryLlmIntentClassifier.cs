using System.Text.Json;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Mystira.StoryGenerator.Contracts.Chat;
using Mystira.StoryGenerator.Contracts.Configuration;
using Mystira.StoryGenerator.Contracts.Intent;
using Mystira.StoryGenerator.Domain.Services;

namespace Mystira.StoryGenerator.Llm.Services.StoryIntentClassification;

public class StoryLlmIntentClassifier : ILlmIntentClassificationService
{
    private readonly IntentRouterSettings _settings;
    private readonly ILlmServiceFactory _llmServiceFactory;
    private readonly ILogger<StoryLlmIntentClassifier> _logger;

    private const string ClassificationPrompt = @"
You are the Mystira RAG intent classifier.
Your ONLY job is to classify a single user instruction into:

categories (one or more of):
- story_generation  → generating or changing a Mystira story
- validation        → checking YAML/JSON/schema correctness or constraints
- autofix           → automatically fixing problems (schema, structure, safety)
- summarization     → summarizing or explaining an existing story or JSON
- safety            → questions about safety rules, child-appropriateness, content policies
- meta              → questions about schemas, documentation, how the system works, or general help

instructionTypes (one or more of):
- story_generate_initial  → create a new story from scratch
- story_generate_refine   → adjust or improve an existing story
- story_validate          → check YAML/JSON/schema or rule compliance
- story_autofix           → automatically fix errors/problems in a story/YAML
- story_summarize         → summarize or explain a story or JSON
- help                    → general help about what you can do, how to use the system
- schema_docs             → questions about fields, schema structure, allowed values
- safety_policy           → questions about safety, age-appropriateness, content limitations
- requirements            → “what are the requirements” / “what must a story do” / “include requirements”
- guidelines              → “what are the guidelines” / best practices, suggestions / “include guidelines”

There should generally only be one category. Only select more than one if you are absolutely sure.
More than one instructionTypes may often apply; for example, the user might ask to both generate and validate, or
refine while satisfying requirements.

Return ONLY a JSON object with the following format, no additional text:
{
  ""categories"": [""<category1>"", ""<category2>""],
  ""instructionTypes"": [""<instructionType1>"", ""<instructionType2>""]
}

Rules:
- ""categories"" MUST contain at least one allowed value from the list above.
- ""instructionTypes"" MUST contain at least one allowed value from the list above.
- If the intent is clearly focused on ONE operation, return arrays of length 1.
- If the user is clearly asking for two sequential operations, you MAY include both
  (e.g., generate + summarize, validate + autofix).
- If the user is only asking a question ABOUT the system (requirements, schema, config, safety),
  do NOT use story_generate_initial/refine/validate/autofix; instead use:
  - requirements, guidelines, schema_docs, or safety_policy as appropriate.
- If unsure between two closely related options, choose the ONE that best matches the primary action.

Heuristics:
- Mentions of “create”, “generate”, “make a story”, “write a story” → story_generation + story_generate_initial
- Mentions of “improve”, “update”, “tweak”, “rewrite this story”, “make it more X” → story_generation + story_generate_refine
- Mentions of “validate”, “check”, “is this valid”, “schema errors”, “lint this YAML/JSON” → validation + story_validate
- Mentions of “fix”, “auto-fix”, “repair this YAML/story”, “correct issues” → autofix + story_autofix
- Mentions of “summarize”, “shorter version”, “TL;DR”, “explain this story/JSON” → summarization + story_summarize
- Mentions of “config”, “settings”, “model”, “API key”, “switch provider” → config_view or config_update
- Mentions of “schema”, “fields”, “what does this field mean”, “allowed values” → meta + schema_docs
- Mentions of “requirements”, “what must a story do”, “constraints” → story_generation + requirements
- Mentions of “guidelines”, “best practices”, “how should I design” → story_generation + guidelines
- Mentions of “safety”, “age-appropriate”, “what content is allowed”, “is this safe for kids” → safety + safety_policy
- Mentions of “what can you do”, “how do I use this system” → meta + help

Examples:

User: ""Create a new story about a knight""
Response:
{""categories"": [""story_generation""], ""instructionTypes"": [""story_generate_initial""]}

User: ""Generate a scary forest adventure story for an 8-year-old""
Response:
{""categories"": [""story_generation""], ""instructionTypes"": [""story_generate_initial""]}


User: ""Generate a story ensuring that you include relevant requirements and guidelines""
Response:
{""categories"": [""story_generation""], ""instructionTypes"": [""story_generate_initial"", ""requirements"", ""guidelines""]}

User: ""Update this story to be less scary but keep the same characters""
Response:
{""categories"": [""story_generation""], ""instructionTypes"": [""story_generate_refine""]}

User: ""Validate my JSON""
Response:
{""categories"": [""validation""], ""instructionTypes"": [""story_validate""]}

User: ""Check this YAML against the Mystira story schema""
Response:
{""categories"": [""validation""], ""instructionTypes"": [""story_validate""]}

User: ""Auto-fix any schema errors in this YAML""
Response:
{""categories"": [""autofix""], ""instructionTypes"": [""story_autofix""]}

User: ""Fix any issues in this story so it passes validation""
Response:
{""categories"": [""autofix""], ""instructionTypes"": [""story_autofix""]}

User: ""Summarize this story for parents""
Response:
{""categories"": [""summarization""], ""instructionTypes"": [""story_summarize""]}

User: ""What are the requirements for story generation?""
Response:
{""categories"": [""story_generation""], ""instructionTypes"": [""requirements""]}

User: ""Give me guidelines for writing a Mystira story for 6–9 year olds""
Response:
{""categories"": [""story_generation""], ""instructionTypes"": [""guidelines""]}

User: ""What are the safety rules for content?""
Response:
{""categories"": [""safety""], ""instructionTypes"": [""safety_policy""]}

User: ""What can you do?""
Response:
{""categories"": [""meta""], ""instructionTypes"": [""help""]}

User: ""Generate a new story and then summarize it""
Response:
{""categories"": [""story_generation"", ""summarization""], ""instructionTypes"": [""story_generate_initial"", ""story_summarize""]}

Now classify this user instruction:
";

    public StoryLlmIntentClassifier(
        IOptions<AiSettings> aiOptions,
        ILlmServiceFactory llmServiceFactory,
        ILogger<StoryLlmIntentClassifier> logger)
    {
        _settings = aiOptions.Value.IntentRouter;
        _llmServiceFactory = llmServiceFactory;
        _logger = logger;
    }

    public async Task<IntentClassification?> ClassifyAsync(string sceneContent, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(sceneContent))
        {
            _logger.LogWarning("Intent classification requested with empty query");
            return null;
        }

        if (!_settings.IsConfigured)
        {
            _logger.LogDebug("Intent classifier is not configured, skipping classification");
            return null;
        }

        try
        {
            var deploymentNameOrModelId = _settings.DeploymentName ?? _settings.ModelId;
            var llmService = _llmServiceFactory.GetService(_settings.Provider!, deploymentNameOrModelId);
            if (llmService == null)
            {
                _logger.LogWarning("LLM provider {Provider} not available for intent routing", _settings.Provider);
                return null;
            }

            var request = new ChatCompletionRequest
            {
                Provider = _settings.Provider,
                ModelId = _settings.ModelId,
                Model = deploymentNameOrModelId,
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
                        Content = sceneContent,
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
