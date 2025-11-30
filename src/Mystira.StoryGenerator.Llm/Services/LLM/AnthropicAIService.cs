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
        var configuredName = _settings.Anthropic.ModelName;

        // If no models list configured, fall back to a single synthetic entry for the configured model name
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
                Capabilities = new List<string> { "chat", "story-generation" }
            };
            return new List<ChatModelInfo> { model };
        }

        // If a configured model name is provided, return only that specific model when possible
        if (!string.IsNullOrWhiteSpace(configuredName))
        {
            var selected = models.FirstOrDefault(m => string.Equals(m.Name, configuredName, StringComparison.OrdinalIgnoreCase));
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
                        Capabilities = selected.Capabilities ?? new List<string> { "chat", "story-generation" }
                    }
                };
            }

            // Not in list: return a synthetic single entry with a mapped friendly name
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
                    Capabilities = new List<string> { "chat", "story-generation" }
                }
            };
        }

        // No specific configured model: return all configured entries
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
            Capabilities = m.Capabilities ?? new List<string> { "chat", "story-generation" }
        });
    }

    private static string GetDisplayNameForModel(string modelName)
    {
        return modelName.ToLowerInvariant() switch
        {
            // Common Anthropic model ids mapping to friendly names
            var name when name.Contains("claude-3-5-sonnet") => "Claude 3.5 Sonnet",
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
                BaseUrl = new Uri("https://dev-swe-ai-mystira-stor-resource.services.ai.azure.com/anthropic"),
                Timeout = TimeSpan.FromSeconds(300)
            });

            var messages = new List<MessageParam>();
            foreach (var msg in request.Messages)
            {
                // Anthropic Messages API expects content as a list of content blocks
                var contentBlocks = new List<ContentBlockParam>
                {
                    new TextBlockParam
                    {
                        Text = msg.Content
                    }
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
                // Anthropic expects `System` as a SystemModel with content blocks
                System = string.IsNullOrWhiteSpace(systemPrompt)
                    ? null
                    : new SystemModel(new List<TextBlockParam>
                    {
                        new TextBlockParam { Text = systemPrompt }
                    })
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

    // Returns a model-specific endpoint if configured; otherwise empty string.
    private string ResolveEndpoint(string? modelName)
    {
        if (string.IsNullOrWhiteSpace(modelName)) return string.Empty;

        var models = _settings.Anthropic.Models;
        if (models == null || models.Count == 0) return string.Empty;

        var match = models.FirstOrDefault(m => string.Equals(m.Name, modelName, StringComparison.OrdinalIgnoreCase));
        return match?.Endpoint ?? string.Empty;
    }

    private static string ExtractTextContent(IEnumerable<ContentBlock>? contentBlocks)
    {
        if (contentBlocks == null)
            return string.Empty;

        var first = contentBlocks.FirstOrDefault();
        if (first == null)
            return string.Empty;

        // Anthropic SDK uses a wrapper ContentBlock with a Value that holds the concrete block
        // type (e.g., TextBlock). We only support text blocks here.
        if (first.Value is TextBlock tb)
            return tb.Text ?? string.Empty;

        throw new NotSupportedException($"Unsupported content block type: {first.Value?.GetType().Name ?? first.GetType().Name}");
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
