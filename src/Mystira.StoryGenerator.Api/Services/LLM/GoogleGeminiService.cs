using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Options;
using Mystira.StoryGenerator.Contracts.Chat;
using Mystira.StoryGenerator.Contracts.Configuration;

namespace Mystira.StoryGenerator.Api.Services.LLM;

/// <summary>
/// Google Gemini implementation of <see cref="ILLMService"/>.
/// </summary>
public class GoogleGeminiService : ILLMService
{
    private readonly HttpClient _httpClient;
    private readonly GoogleGeminiSettings _settings;
    private readonly ILogger<GoogleGeminiService> _logger;

    public string ProviderName => "google-gemini";

    public GoogleGeminiService(HttpClient httpClient, IOptions<AiSettings> options, ILogger<GoogleGeminiService> logger)
    {
        _httpClient = httpClient;
        _settings = options.Value.GoogleGemini;
        _logger = logger;
    }

    public bool IsAvailable()
    {
        return !string.IsNullOrWhiteSpace(_settings.ApiKey);
    }

    public async Task<ChatCompletionResponse> CompleteAsync(ChatCompletionRequest request, CancellationToken cancellationToken = default)
    {
        if (!IsAvailable())
        {
            return CreateErrorResponse("Google Gemini service is not properly configured");
        }

        var modelName = ResolveModelName(request);
        if (string.IsNullOrWhiteSpace(modelName))
        {
            return CreateErrorResponse("A Gemini model name must be provided");
        }

        try
        {
            var requestBody = CreateGeminiRequest(request);
            var jsonContent = JsonSerializer.Serialize(requestBody, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            });

            var httpRequest = new HttpRequestMessage(HttpMethod.Post, GetEndpointUrl(modelName))
            {
                Content = new StringContent(jsonContent, Encoding.UTF8, "application/json")
            };

            _logger.LogDebug("Sending request to Google Gemini model {Model}: {RequestBody}", modelName, jsonContent);

            var response = await _httpClient.SendAsync(httpRequest, cancellationToken);
            var responseContent = await response.Content.ReadAsStringAsync(cancellationToken);

            _logger.LogDebug("Received response from Google Gemini: {StatusCode} - {ResponseContent}",
                response.StatusCode, responseContent);

            if (!response.IsSuccessStatusCode)
            {
                return CreateErrorResponse($"Google Gemini API error: {response.StatusCode} - {responseContent}");
            }

            var geminiResponse = JsonSerializer.Deserialize<GeminiResponse>(responseContent, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            var content = geminiResponse?.Candidates?.FirstOrDefault()?.Content?.Parts?.FirstOrDefault()?.Text ?? string.Empty;

            return new ChatCompletionResponse
            {
                Content = content,
                Model = modelName,
                Provider = ProviderName,
                Usage = geminiResponse?.UsageMetadata != null ? new ChatCompletionUsage
                {
                    PromptTokens = geminiResponse.UsageMetadata.PromptTokenCount,
                    CompletionTokens = geminiResponse.UsageMetadata.CandidatesTokenCount,
                    TotalTokens = geminiResponse.UsageMetadata.TotalTokenCount
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

    private static object CreateGeminiRequest(ChatCompletionRequest request)
    {
        var contents = new List<object>();

        if (!string.IsNullOrWhiteSpace(request.SystemPrompt))
        {
            contents.Add(new
            {
                role = "user",
                parts = new[]
                {
                    new { text = request.SystemPrompt }
                }
            });
        }

        foreach (var message in request.Messages)
        {
            var role = message.MessageType == ChatMessageType.AI ? "model" : "user";
            contents.Add(new
            {
                role,
                parts = new[]
                {
                    new { text = message.Content }
                }
            });
        }

        return new
        {
            contents,
            generationConfig = new
            {
                temperature = request.Temperature,
                maxOutputTokens = request.MaxTokens
            }
        };
    }

    private string? ResolveModelName(ChatCompletionRequest request)
    {
        if (!string.IsNullOrWhiteSpace(request.Model))
        {
            return request.Model;
        }

        if (!string.IsNullOrWhiteSpace(request.ModelId))
        {
            return request.ModelId;
        }

        return string.IsNullOrWhiteSpace(_settings.Model) ? null : _settings.Model;
    }

    private string GetEndpointUrl(string modelName)
    {
        return $"https://generativelanguage.googleapis.com/v1beta/models/{modelName}:generateContent?key={_settings.ApiKey}";
    }

    private ChatCompletionResponse CreateErrorResponse(string error)
    {
        return new ChatCompletionResponse
        {
            Success = false,
            Error = error,
            Provider = ProviderName
        };
    }

    private class GeminiResponse
    {
        public List<Candidate>? Candidates { get; set; }
        public UsageMetadata? UsageMetadata { get; set; }
    }

    private class Candidate
    {
        public Content? Content { get; set; }
    }

    private class Content
    {
        public List<Part>? Parts { get; set; }
    }

    private class Part
    {
        public string? Text { get; set; }
    }

    private class UsageMetadata
    {
        public int PromptTokenCount { get; set; }
        public int CandidatesTokenCount { get; set; }
        public int TotalTokenCount { get; set; }
    }
}
