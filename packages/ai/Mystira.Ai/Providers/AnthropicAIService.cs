using System.Globalization;
using System.Text;
using Anthropic;
using Anthropic.Core;
using Anthropic.Models.Messages;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Mystira.Ai.Abstractions;
using Mystira.Ai.Configuration;
using Mystira.Contracts.StoryGenerator.Chat;

namespace Mystira.Ai.Providers;

/// <summary>
/// Anthropic Claude implementation of ILLMService.
/// </summary>
public class AnthropicAIService : ILLMService
{
    private readonly AiSettings _settings;
    private readonly ILogger<AnthropicAIService> _logger;
    private string? _modelNameOrId;

    /// <inheritdoc />
    public string ProviderName => "anthropic";

    /// <inheritdoc />
    public string? DeploymentNameOrModelId => _modelNameOrId;

    /// <summary>
    /// Creates a new Anthropic AI service.
    /// </summary>
    public AnthropicAIService(IOptions<AiSettings> options, ILogger<AnthropicAIService> logger)
    {
        _settings = options.Value;
        _logger = logger;
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

            var clientOptions = new ClientOptions
            {
                APIKey = _settings.Anthropic.ApiKey,
                Timeout = TimeSpan.FromSeconds(300)
            };

            // Use custom base URL if configured
            if (!string.IsNullOrWhiteSpace(_settings.Anthropic.BaseUrl))
            {
                clientOptions.BaseUrl = new Uri(_settings.Anthropic.BaseUrl);
            }

            var client = new AnthropicClient(clientOptions);

            var messages = new List<MessageParam>();
            foreach (var msg in request.Messages)
            {
                var contentBlocks = new List<ContentBlockParam>
                {
                    new TextBlockParam { Text = msg.Content }
                };

                messages.Add(new MessageParam
                {
                    Role = msg.MessageType == ChatMessageType.User ? "user" : "assistant",
                    Content = contentBlocks
                });
            }

            var systemPrompt = request.SystemPrompt ?? string.Empty;

            var response = await client.Messages.Create(new MessageCreateParams
            {
                Model = modelName,
                MaxTokens = request.MaxTokens,
                Temperature = (float)request.Temperature,
                Messages = messages,
                System = string.IsNullOrWhiteSpace(systemPrompt)
                    ? null
                    : new SystemModel(new List<TextBlockParam>
                    {
                        new TextBlockParam { Text = systemPrompt }
                    })
            }, cancellationToken: cancellationToken);

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
                        PromptTokens = (int)response.Usage.InputTokens,
                        CompletionTokens = (int)response.Usage.OutputTokens,
                        TotalTokens = (int)(response.Usage.InputTokens + response.Usage.OutputTokens)
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

    private static string ExtractTextContent(IEnumerable<ContentBlock>? contentBlocks)
    {
        if (contentBlocks == null)
            return string.Empty;

        var first = contentBlocks.FirstOrDefault();
        if (first == null)
            return string.Empty;

        if (first.Value is TextBlock tb)
            return tb.Text ?? string.Empty;

        throw new NotSupportedException(
            $"Unsupported content block type: {first.Value?.GetType().Name ?? first.GetType().Name}");
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
}
