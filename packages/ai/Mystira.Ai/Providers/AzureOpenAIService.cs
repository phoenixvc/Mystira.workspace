using System.ClientModel;
using System.Globalization;
using System.Text;
using Azure.AI.OpenAI;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Mystira.Ai.Abstractions;
using Mystira.Ai.Configuration;
using Mystira.Contracts.StoryGenerator.Chat;
using Mystira.Contracts.StoryGenerator.Extensions;
using OpenAI.Chat;

namespace Mystira.Ai.Providers;

/// <summary>
/// Azure OpenAI implementation of ILLMService.
/// </summary>
public class AzureOpenAIService : ILLMService
{
    private readonly AiSettings _settings;
    private readonly ILogger<AzureOpenAIService> _logger;
    private string? _deploymentNameOrModelId;

    /// <inheritdoc />
    public string ProviderName => "azure-openai";

    /// <inheritdoc />
    public string? DeploymentNameOrModelId => _deploymentNameOrModelId;

    /// <summary>
    /// Creates a new Azure OpenAI service.
    /// </summary>
    public AzureOpenAIService(IOptions<AiSettings> options, ILogger<AzureOpenAIService> logger)
    {
        _settings = options.Value;
        _logger = logger;
    }

    /// <summary>
    /// Sets the deployment name or model ID for this service instance.
    /// </summary>
    internal void SetDeploymentNameOrModelId(string? deploymentNameOrModelId)
    {
        _deploymentNameOrModelId = deploymentNameOrModelId;
    }

    /// <inheritdoc />
    public bool IsAvailable()
    {
        return !string.IsNullOrWhiteSpace(_settings.AzureOpenAI.ApiKey) &&
               !string.IsNullOrWhiteSpace(_settings.AzureOpenAI.Endpoint) &&
               !string.IsNullOrWhiteSpace(_settings.AzureOpenAI.DeploymentName);
    }

    /// <inheritdoc />
    public IEnumerable<ChatModelInfo> GetAvailableModels()
    {
        if (!IsAvailable())
        {
            return Enumerable.Empty<ChatModelInfo>();
        }

        var deployments = _settings.AzureOpenAI.Deployments;
        if (deployments == null || !deployments.Any())
        {
            var model = new ChatModelInfo
            {
                Id = _settings.AzureOpenAI.DeploymentName,
                DisplayName = GetDisplayNameForDeployment(_settings.AzureOpenAI.DeploymentName),
                Description = "Azure OpenAI GPT model deployment",
                MaxTokens = 4096,
                DefaultTemperature = 0.7,
                MinTemperature = 0.0,
                MaxTemperature = 2.0,
                SupportsJsonSchema = true,
                Capabilities = new List<string> { "chat", "json-schema" }
            };
            return new List<ChatModelInfo> { model };
        }

        var configuredName = _settings.AzureOpenAI.DeploymentName;

        if (!string.IsNullOrWhiteSpace(configuredName))
        {
            var selected = deployments.FirstOrDefault(d =>
                string.Equals(d.Name, configuredName, StringComparison.OrdinalIgnoreCase));
            if (selected != null)
            {
                return new[]
                {
                    new ChatModelInfo
                    {
                        Id = selected.Name,
                        DisplayName = selected.DisplayName,
                        Description = $"Azure OpenAI {selected.DisplayName} deployment",
                        MaxTokens = selected.MaxTokens,
                        DefaultTemperature = selected.DefaultTemperature,
                        MinTemperature = 0.0,
                        MaxTemperature = 2.0,
                        SupportsJsonSchema = selected.SupportsJsonSchema,
                        Capabilities = selected.Capabilities ?? new List<string> { "chat" }
                    }
                };
            }

            return new[]
            {
                new ChatModelInfo
                {
                    Id = configuredName,
                    DisplayName = GetDisplayNameForDeployment(configuredName),
                    Description = "Azure OpenAI GPT model deployment",
                    MaxTokens = 4096,
                    DefaultTemperature = 0.7,
                    MinTemperature = 0.0,
                    MaxTemperature = 2.0,
                    SupportsJsonSchema = true,
                    Capabilities = new List<string> { "chat", "json-schema" }
                }
            };
        }

        return deployments.Select(deployment => new ChatModelInfo
        {
            Id = deployment.Name,
            DisplayName = deployment.DisplayName,
            Description = $"Azure OpenAI {deployment.DisplayName} deployment",
            MaxTokens = deployment.MaxTokens,
            DefaultTemperature = deployment.DefaultTemperature,
            MinTemperature = 0.0,
            MaxTemperature = 2.0,
            SupportsJsonSchema = deployment.SupportsJsonSchema,
            Capabilities = deployment.Capabilities ?? new List<string> { "chat" }
        });
    }

    private static string GetDisplayNameForDeployment(string deploymentName)
    {
        return deploymentName.ToLowerInvariant() switch
        {
            var name when name.Contains("gpt-4") => "GPT-4",
            var name when name.Contains("gpt-5.1") => "GPT-5.1",
            var name when name.Contains("gpt-5-nano") => "GPT-5-Nano",
            var name when name.Contains("claude-sonnet-4-5") => "Claude-Sonnet-4.5",
            var name when name.Contains("gpt-3.5") => "GPT-3.5 Turbo",
            var name when name.Contains("gpt-35") => "GPT-3.5 Turbo",
            _ => deploymentName
        };
    }

    /// <inheritdoc />
    public async Task<ChatCompletionResponse> CompleteAsync(
        ChatCompletionRequest request,
        CancellationToken cancellationToken = default)
    {
        if (!IsAvailable())
            return CreateErrorResponse("Azure OpenAI service is not properly configured");

        try
        {
            var deploymentName = ResolveDeploymentName(request);
            var endpoint = ResolveEndpoint(deploymentName);

            var azureClient = new AzureOpenAIClient(
                new Uri(endpoint),
                new ApiKeyCredential(_settings.AzureOpenAI.ApiKey),
                new AzureOpenAIClientOptions
                {
                    NetworkTimeout = new TimeSpan(0, 0, 5, 0)
                });
            var chatClient = azureClient.GetChatClient(deploymentName);

            var messages = request.ToOpenAiChatMessages();
            var options = BuildOptions(request, _logger);

            var azureResponse = await chatClient.CompleteChatAsync(
                messages,
                options: options,
                cancellationToken: cancellationToken);
            var response = azureResponse.Value;

            var content = response?.Content?.FirstOrDefault()?.Text ?? string.Empty;
            var cleanContent = Sanitize(content);

            var finishReason = response?.FinishReason.ToString();
            var isIncomplete = finishReason == "Length";

            return new ChatCompletionResponse
            {
                Content = cleanContent ?? string.Empty,
                Model = deploymentName,
                Provider = ProviderName,
                Usage = response?.Usage != null
                    ? new ChatCompletionUsage
                    {
                        PromptTokens = response.Usage.InputTokenCount,
                        CompletionTokens = response.Usage.OutputTokenCount,
                        TotalTokens = response.Usage.TotalTokenCount
                    }
                    : null,
                Success = true,
                FinishReason = finishReason,
                IsIncomplete = isIncomplete
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calling Azure OpenAI API");
            return CreateErrorResponse($"Azure OpenAI service error: {ex.Message}");
        }
    }

    private string ResolveDeploymentName(ChatCompletionRequest request)
    {
        if (!string.IsNullOrEmpty(request.Model))
        {
            _deploymentNameOrModelId = request.Model;
            return request.Model;
        }

        if (!string.IsNullOrWhiteSpace(_deploymentNameOrModelId))
            return _deploymentNameOrModelId;

        _deploymentNameOrModelId = _settings.AzureOpenAI.DeploymentName;
        return _settings.AzureOpenAI.DeploymentName;
    }

    private string ResolveEndpoint(string deploymentName)
    {
        if (!string.IsNullOrWhiteSpace(deploymentName) && _settings.AzureOpenAI.Deployments != null)
        {
            var deployment = _settings.AzureOpenAI.Deployments
                .FirstOrDefault(d => d.Name == deploymentName);
            if (deployment != null && !string.IsNullOrWhiteSpace(deployment.Endpoint))
            {
                return deployment.Endpoint;
            }
        }

        return _settings.AzureOpenAI.Endpoint;
    }

    /// <summary>
    /// Builds chat completion options with optional JSON schema format.
    /// Exposed for unit testing.
    /// </summary>
    public static ChatCompletionOptions? BuildOptions(ChatCompletionRequest request, ILogger logger)
    {
        if (request.JsonSchemaFormat is not null &&
            !string.IsNullOrWhiteSpace(request.JsonSchemaFormat.SchemaJson))
        {
            try
            {
                return new ChatCompletionOptions
                {
                    ResponseFormat = ChatResponseFormat.CreateJsonSchemaFormat(
                        jsonSchemaFormatName: string.IsNullOrWhiteSpace(request.JsonSchemaFormat.FormatName)
                            ? "mystira-json"
                            : request.JsonSchemaFormat.FormatName,
                        jsonSchema: BinaryData.FromString(request.JsonSchemaFormat.SchemaJson),
                        jsonSchemaIsStrict: request.JsonSchemaFormat.IsStrict
                    )
                };
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex,
                    "Failed to configure JSON schema response format. Falling back to default response format.");
                return null;
            }
        }

        return null;
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
            Provider = "azure-openai"
        };
    }
}
