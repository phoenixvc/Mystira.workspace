using Google.Cloud.AIPlatform.V1;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Logging;
using Mystira.StoryGenerator.Contracts.Chat;
using Mystira.StoryGenerator.Contracts.Configuration;
using Mystira.StoryGenerator.Domain.Services;
using Mystira.StoryGenerator.Contracts.Extensions;
using System.ClientModel;

namespace Mystira.StoryGenerator.Llm.Services.LLM;

/// <summary>
/// Google Gemini implementation of ILLMService
/// </summary>
public class GoogleGeminiService : ILLMService
{
    private readonly AiSettings _settings;
    private readonly ILogger<GoogleGeminiService> _logger;

    public string ProviderName => "google-gemini";

    public GoogleGeminiService(IOptions<AiSettings> options, ILogger<GoogleGeminiService> logger)
    {
        _settings = options.Value;
        _logger = logger;
    }

    public bool IsAvailable()
    {
        return !string.IsNullOrWhiteSpace(_settings.GoogleGemini.ApiKey) &&
               !string.IsNullOrWhiteSpace(_settings.GoogleGemini.Model);
    }

    public IEnumerable<ChatModelInfo> GetAvailableModels()
    {
        if (!IsAvailable())
        {
            return Enumerable.Empty<ChatModelInfo>();
        }

        // For Google Gemini, we return the configured model as the available model
        // In a real implementation, you might call the Google AI API to list available models
        var model = new ChatModelInfo
        {
            Id = _settings.GoogleGemini.Model,
            DisplayName = GetDisplayNameForModel(_settings.GoogleGemini.Model),
            Description = "Google Gemini language model",
            MaxTokens = 8192, // Gemini typically supports higher token limits
            DefaultTemperature = 0.7,
            MinTemperature = 0.0,
            MaxTemperature = 2.0,
            SupportsJsonSchema = false, // Gemini has limited JSON schema support
            Capabilities = new List<string> { "chat", "story-generation" }
        };

        return new List<ChatModelInfo> { model };
    }

    private static string GetDisplayNameForModel(string modelName)
    {
        // Convert model name to a more user-friendly display name
        return modelName.ToLowerInvariant() switch
        {
            "gemini-pro" => "Gemini Pro",
            "gemini-pro-vision" => "Gemini Pro Vision",
            "gemini-1.5-pro" => "Gemini 1.5 Pro",
            "gemini-1.5-flash" => "Gemini 1.5 Flash",
            _ => modelName
        };
    }

    public async Task<ChatCompletionResponse> CompleteAsync(ChatCompletionRequest request, CancellationToken cancellationToken = default)
    {
        if (!IsAvailable()) return CreateErrorResponse("Google Gemini service is not properly configured");

        try
        {
            // Initialize the client
            var client = new PredictionServiceClientBuilder
            {
                ApiKey = _settings.GoogleGemini.ApiKey
            }.Build();

            // Convert messages to Gemini format
            var contents = request.ToGeminiContents();

            // Generate content
            var response = await client.GenerateContentAsync(
                _settings.GoogleGemini.Model,
                contents,
                cancellationToken: cancellationToken);

            return new ChatCompletionResponse
            {
                Content = response.Candidates.FirstOrDefault()?.Content.Parts.FirstOrDefault()?.Text ?? string.Empty,
                Model = _settings.GoogleGemini.Model,
                Provider = ProviderName,
                Usage = response.UsageMetadata != null ? new ChatCompletionUsage
                {
                    PromptTokens = response.UsageMetadata.PromptTokenCount ?? 0,
                    CompletionTokens = response.UsageMetadata.CandidatesTokenCount ?? 0,
                    TotalTokens = response.UsageMetadata.TotalTokenCount ?? 0
                } : null,
                Success = true
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calling Google Gemini API");
            return CreateErrorResponse($"Google Gemini service error: {ex.Message}");
        }
    }

    private static ChatCompletionResponse CreateErrorResponse(string error)
    {
        return new ChatCompletionResponse
        {
            Success = false,
            Error = error,
            Provider = "google-gemini"
        };
    }
}