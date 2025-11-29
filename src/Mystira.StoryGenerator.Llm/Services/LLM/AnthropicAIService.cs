using System.Globalization;
using System.Text;
using Anthropic.Core;
using Anthropic.Models.Messages;
using global::Anthropic;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Logging;
using Mystira.StoryGenerator.Contracts.Chat;
using Mystira.StoryGenerator.Contracts.Configuration;
using Mystira.StoryGenerator.Domain.Services;

namespace Mystira.StoryGenerator.Llm.Services.LLM;

/// <summary>
/// Anthropic Claude implementation of ILLMService
/// </summary>
public class AnthropicAIService : ILLMService
{
    private readonly AiSettings _settings;
    private readonly ILogger<AnthropicAIService> _logger;
    private string? _modelNameOrId;

    public string ProviderName => "anthropic";
    public string? DeploymentNameOrModelId => _modelNameOrId;

    public AnthropicAIService(IOptions<AiSettings> options, ILogger<AnthropicAIService> logger)
    {
        _settings = options.Value;
        _logger = logger;
    }

    internal void SetModelNameOrId(string? modelNameOrId)
    {
        _modelNameOrId = modelNameOrId;
    }

    public bool IsAvailable()
    {
        return !string.IsNullOrWhiteSpace(_settings.Anthropic.ApiKey) &&
               !string.IsNullOrWhiteSpace(_settings.Anthropic.ModelName);
    }

    public IEnumerable<ChatModelInfo> GetAvailableModels()
    {
        if (!IsAvailable())
        {
            return Enumerable.Empty<ChatModelInfo>();
        }

        var models = _settings.Anthropic.Models;
        if (models == null || !models.Any())
        {
            var model = new ChatModelInfo
            {
                Id = _settings.Anthropic.ModelName,
                DisplayName = GetDisplayNameForModel(_settings.Anthropic.ModelName),
                Description = "Anthropic Claude model",
                MaxTokens = 4096,
                DefaultTemperature = 0.7,
                MinTemperature = 0.0,
                MaxTemperature = 1.0,
                SupportsJsonSchema = false,
                Capabilities = new List<string> { "chat", "story-generation" }
            };
            return new List<ChatModelInfo> { model };
        }

        var chatModels = models.Select(m => new ChatModelInfo
        {
            Id = m.Name,
            DisplayName = m.DisplayName,
            Description = $"Anthropic {m.DisplayName} model",
            MaxTokens = m.MaxTokens,
            DefaultTemperature = m.DefaultTemperature,
            MinTemperature = 0.0,
            MaxTemperature = 1.0,
            SupportsJsonSchema = m.SupportsJsonMode,
            Capabilities = m.Capabilities ?? new List<string> { "chat", "story-generation" }
        });

        return chatModels;
    }

    private static string GetDisplayNameForModel(string modelName)
    {
        return modelName.ToLowerInvariant() switch
        {
            var name when name.Contains("claude-sonnet-4-5") => "Claude Sonnet 4.5",
            var name when name.Contains("claude-3-opus") => "Claude 3 Opus",
            var name when name.Contains("claude-3-sonnet") => "Claude 3 Sonnet",
            var name when name.Contains("claude-3-haiku") => "Claude 3 Haiku",
            _ => modelName
        };
    }

    public async Task<ChatCompletionResponse> CompleteAsync(ChatCompletionRequest request, CancellationToken cancellationToken = default)
    {
        if (!IsAvailable()) return CreateErrorResponse("Anthropic service is not properly configured");

        try
        {
            var modelName = ResolveModelName(request);

            var client = new AnthropicClient(new ClientOptions
            {
                APIKey = _settings.Anthropic.ApiKey,
                BaseUrl = new Uri(""),
                Timeout = TimeSpan.FromSeconds(300)
            });

            var messages = new List<MessageParam>();
            foreach (var msg in request.Messages)
            {
                messages.Add(new MessageParam
                {
                    Role = msg.MessageType == ChatMessageType.User ? "user" : "assistant",
                    Content = msg.Content
                });
            }

            var systemPrompt = request.SystemPrompt ?? string.Empty;

            var response = await client.Messages.Create(new MessageCreateParams
            {
                Model = modelName,
                MaxTokens = request.MaxTokens,
                Temperature = (float)request.Temperature,
                Messages = messages,
                System = string.IsNullOrWhiteSpace(systemPrompt) ? null : systemPrompt
            }, cancellationToken: cancellationToken);

            var content = ExtractTextContent(response.Content);
            var cleanContent = Sanitize(content);

            return new ChatCompletionResponse
            {
                Content = cleanContent ?? string.Empty,
                Model = modelName,
                Provider = ProviderName,
                Usage = response.Usage != null ? new ChatCompletionUsage
                {
                    PromptTokens = (int)response.Usage.InputTokens,
                    CompletionTokens = (int)response.Usage.OutputTokens,
                    TotalTokens = (int)(response.Usage.InputTokens + response.Usage.OutputTokens)
                } : null,
                Success = true
            };

            string? Sanitize(string? input)
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

        if (!string.IsNullOrWhiteSpace(_modelNameOrId)) return _modelNameOrId;

        _modelNameOrId = _settings.Anthropic.ModelName;
        return _settings.Anthropic.ModelName;
    }

    private static string ExtractTextContent(IEnumerable<ContentBlock>? contentBlocks)
    {
        if (contentBlocks == null)
            return string.Empty;

        var textContent = contentBlocks
            .OfType<TextBlock>()
            .FirstOrDefault();

        return textContent?.Text ?? string.Empty;
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
