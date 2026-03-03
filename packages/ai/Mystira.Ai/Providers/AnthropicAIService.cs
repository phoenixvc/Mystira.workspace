using System.Globalization;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Mystira.Ai.Abstractions;
using Mystira.Ai.Configuration;
using Mystira.Contracts.StoryGenerator.Chat;

namespace Mystira.Ai.Providers;

/// <summary>
/// Anthropic Claude implementation of ILLMService using the official REST API.
/// </summary>
public class AnthropicAIService : ILLMService
{
    private readonly AiSettings _settings;
    private readonly ILogger<AnthropicAIService> _logger;
    private readonly HttpClient _httpClient;
    private string? _modelNameOrId;

    private const string DefaultBaseUrl = "https://api.anthropic.com";
    private const string ApiVersion = "2023-06-01";

    /// <inheritdoc />
    public string ProviderName => "anthropic";

    /// <inheritdoc />
    public string? DeploymentNameOrModelId => _modelNameOrId;

    /// <summary>
    /// Creates a new Anthropic AI service.
    /// </summary>
    public AnthropicAIService(IOptions<AiSettings> options, ILogger<AnthropicAIService> logger)
        : this(options, logger, new HttpClient())
    {
    }

    /// <summary>
    /// Creates a new Anthropic AI service with a custom HttpClient.
    /// </summary>
    public AnthropicAIService(IOptions<AiSettings> options, ILogger<AnthropicAIService> logger, HttpClient httpClient)
    {
        _settings = options.Value;
        _logger = logger;
        _httpClient = httpClient;
        _httpClient.Timeout = TimeSpan.FromSeconds(300);
    }

    /// <summary>
    /// Sets the model name or ID for this service instance.
    /// </summary>
    internal void SetModelNameOrId(string? modelNameOrId)
    {
        _modelNameOrId = modelNameOrId;
    }

    /// <inheritdoc />
    public bool IsAvailable()
    {
        return !string.IsNullOrWhiteSpace(_settings.Anthropic.ApiKey) &&
               !string.IsNullOrWhiteSpace(_settings.Anthropic.ModelName);
    }

    /// <inheritdoc />
    public IEnumerable<ChatModelInfo> GetAvailableModels()
    {
        if (!IsAvailable())
        {
            return Enumerable.Empty<ChatModelInfo>();
        }

        var models = _settings.Anthropic.Models;
        var configuredName = _settings.Anthropic.ModelName;

        if (models == null || !models.Any())
        {
            var model = new ChatModelInfo
            {
                Id = configuredName,
                DisplayName = GetDisplayNameForModel(configuredName),
                Description = "Anthropic Claude model",
                MaxTokens = 4096,
                DefaultTemperature = 0.7,
                MinTemperature = 0.0,
                MaxTemperature = 1.0,
                SupportsJsonSchema = false,
                Capabilities = new List<string> { "chat" }
            };
            return new List<ChatModelInfo> { model };
        }

        if (!string.IsNullOrWhiteSpace(configuredName))
        {
            var selected = models.FirstOrDefault(m =>
                string.Equals(m.Name, configuredName, StringComparison.OrdinalIgnoreCase));
            if (selected != null)
            {
                return new[]
                {
                    new ChatModelInfo
                    {
                        Id = selected.Name,
                        DisplayName = selected.DisplayName,
                        Description = $"Anthropic {selected.DisplayName} model",
                        MaxTokens = selected.MaxTokens,
                        DefaultTemperature = selected.DefaultTemperature,
                        MinTemperature = 0.0,
                        MaxTemperature = 1.0,
                        SupportsJsonSchema = selected.SupportsJsonMode,
                        Capabilities = selected.Capabilities ?? new List<string> { "chat" }
                    }
                };
            }

            return new[]
            {
                new ChatModelInfo
                {
                    Id = configuredName,
                    DisplayName = GetDisplayNameForModel(configuredName),
                    Description = "Anthropic Claude model",
                    MaxTokens = 4096,
                    DefaultTemperature = 0.7,
                    MinTemperature = 0.0,
                    MaxTemperature = 1.0,
                    SupportsJsonSchema = false,
                    Capabilities = new List<string> { "chat" }
                }
            };
        }

        return models.Select(m => new ChatModelInfo
        {
            Id = m.Name,
            DisplayName = m.DisplayName,
            Description = $"Anthropic {m.DisplayName} model",
            MaxTokens = m.MaxTokens,
            DefaultTemperature = m.DefaultTemperature,
            MinTemperature = 0.0,
            MaxTemperature = 1.0,
            SupportsJsonSchema = m.SupportsJsonMode,
            Capabilities = m.Capabilities ?? new List<string> { "chat" }
        });
    }

    private static string GetDisplayNameForModel(string modelName)
    {
        return modelName.ToLowerInvariant() switch
        {
            var name when name.Contains("claude-3-5-sonnet") => "Claude 3.5 Sonnet",
            var name when name.Contains("claude-sonnet-4-5") => "Claude Sonnet 4.5",
            var name when name.Contains("claude-3-opus") => "Claude 3 Opus",
            var name when name.Contains("claude-3-sonnet") => "Claude 3 Sonnet",
            var name when name.Contains("claude-3-haiku") => "Claude 3 Haiku",
            _ => modelName
        };
    }

    /// <inheritdoc />
    public async Task<ChatCompletionResponse> CompleteAsync(
        ChatCompletionRequest request,
        CancellationToken cancellationToken = default)
    {
        if (!IsAvailable())
            return CreateErrorResponse("Anthropic service is not properly configured");

        try
        {
            var modelName = ResolveModelName(request);
            var baseUrl = !string.IsNullOrWhiteSpace(_settings.Anthropic.BaseUrl)
                ? _settings.Anthropic.BaseUrl.TrimEnd('/')
                : DefaultBaseUrl;

            var messages = request.Messages.Select(msg => new AnthropicMessage
            {
                Role = msg.MessageType == ChatMessageType.User ? "user" : "assistant",
                Content = msg.Content
            }).ToList();

            var apiRequest = new AnthropicMessagesRequest
            {
                Model = modelName,
                MaxTokens = request.MaxTokens,
                Temperature = request.Temperature,
                Messages = messages,
                System = string.IsNullOrWhiteSpace(request.SystemPrompt) ? null : request.SystemPrompt
            };

            using var httpRequest = new HttpRequestMessage(HttpMethod.Post, $"{baseUrl}/v1/messages");
            httpRequest.Headers.Add("x-api-key", _settings.Anthropic.ApiKey);
            httpRequest.Headers.Add("anthropic-version", ApiVersion);
            httpRequest.Content = JsonContent.Create(apiRequest, options: JsonOptions);

            var httpResponse = await _httpClient.SendAsync(httpRequest, cancellationToken);

            if (!httpResponse.IsSuccessStatusCode)
            {
                var errorBody = await httpResponse.Content.ReadAsStringAsync(cancellationToken);
                _logger.LogError("Anthropic API error: {StatusCode} - {Body}", httpResponse.StatusCode, errorBody);
                return CreateErrorResponse($"Anthropic API error: {httpResponse.StatusCode}");
            }

            var response = await httpResponse.Content.ReadFromJsonAsync<AnthropicMessagesResponse>(JsonOptions, cancellationToken);

            if (response == null)
                return CreateErrorResponse("Failed to parse Anthropic response");

            var content = ExtractTextContent(response.Content);
            var cleanContent = Sanitize(content);

            var finishReason = response.StopReason;
            var isIncomplete = finishReason == "max_tokens";

            return new ChatCompletionResponse
            {
                Content = cleanContent ?? string.Empty,
                Model = modelName,
                Provider = ProviderName,
                Usage = response.Usage != null
                    ? new ChatCompletionUsage
                    {
                        PromptTokens = response.Usage.InputTokens,
                        CompletionTokens = response.Usage.OutputTokens,
                        TotalTokens = response.Usage.InputTokens + response.Usage.OutputTokens
                    }
                    : null,
                Success = true,
                FinishReason = finishReason,
                IsIncomplete = isIncomplete
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calling Anthropic API");
            return CreateErrorResponse($"Anthropic service error: {ex.Message}");
        }
    }

    private string ResolveModelName(ChatCompletionRequest request)
    {
        if (!string.IsNullOrEmpty(request.Model))
        {
            _modelNameOrId = request.Model;
            return request.Model;
        }

        if (!string.IsNullOrWhiteSpace(_modelNameOrId))
            return _modelNameOrId;

        _modelNameOrId = _settings.Anthropic.ModelName;
        return _settings.Anthropic.ModelName;
    }

    private static string ExtractTextContent(List<AnthropicContentBlock>? contentBlocks)
    {
        if (contentBlocks == null || contentBlocks.Count == 0)
            return string.Empty;

        var textBlocks = contentBlocks
            .Where(b => b.Type == "text")
            .Select(b => b.Text)
            .Where(t => t != null);

        return string.Join("", textBlocks);
    }

    private static string? Sanitize(string? input)
    {
        if (input == null) return null;

        var sb = new StringBuilder(input.Length);
        foreach (var ch in input)
        {
            var cat = char.GetUnicodeCategory(ch);
            var isControl = cat == UnicodeCategory.Control;

            if (isControl && ch != '\n' && ch != '\r' && ch != '\t')
                continue;

            sb.Append(ch);
        }

        return sb.ToString();
    }

    private static ChatCompletionResponse CreateErrorResponse(string error)
    {
        return new ChatCompletionResponse
        {
            Success = false,
            Error = error,
            Provider = "anthropic"
        };
    }

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    #region Anthropic API DTOs

    private sealed class AnthropicMessagesRequest
    {
        public string Model { get; set; } = string.Empty;
        public int MaxTokens { get; set; }
        public double Temperature { get; set; }
        public List<AnthropicMessage> Messages { get; set; } = new();
        public string? System { get; set; }
    }

    private sealed class AnthropicMessage
    {
        public string Role { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
    }

    private sealed class AnthropicMessagesResponse
    {
        public string Id { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
        public List<AnthropicContentBlock>? Content { get; set; }
        public string Model { get; set; } = string.Empty;
        public string? StopReason { get; set; }
        public AnthropicUsage? Usage { get; set; }
    }

    private sealed class AnthropicContentBlock
    {
        public string Type { get; set; } = string.Empty;
        public string? Text { get; set; }
    }

    private sealed class AnthropicUsage
    {
        public int InputTokens { get; set; }
        public int OutputTokens { get; set; }
    }

    #endregion
}
