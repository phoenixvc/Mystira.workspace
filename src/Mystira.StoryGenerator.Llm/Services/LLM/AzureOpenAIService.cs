using System.ClientModel;
using System.Globalization;
using System.Text;
using Azure.AI.OpenAI;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Logging;
using Mystira.StoryGenerator.Contracts.Chat;
using Mystira.StoryGenerator.Contracts.Configuration;
using Mystira.StoryGenerator.Domain.Services;
using Mystira.StoryGenerator.Contracts.Extensions;
using OpenAI.Chat;

namespace Mystira.StoryGenerator.Llm.Services.LLM;

/// <summary>
/// Azure OpenAI implementation of ILLMService
/// </summary>
public class AzureOpenAIService : ILLMService
{
    private readonly AiSettings _settings;
    private readonly ILogger<AzureOpenAIService> _logger;
    private string? _deploymentNameOrModelId;

    public string ProviderName => "azure-openai";
    public string? DeploymentNameOrModelId => _deploymentNameOrModelId;

    public AzureOpenAIService(IOptions<AiSettings> options, ILogger<AzureOpenAIService> logger)
    {
        _settings = options.Value;
        _logger = logger;
    }

    internal void SetDeploymentNameOrModelId(string? deploymentNameOrModelId)
    {
        _deploymentNameOrModelId = deploymentNameOrModelId;
    }

    public bool IsAvailable()
    {
        return !string.IsNullOrWhiteSpace(_settings.AzureOpenAI.ApiKey) &&
               !string.IsNullOrWhiteSpace(_settings.AzureOpenAI.Endpoint) &&
               !string.IsNullOrWhiteSpace(_settings.AzureOpenAI.DeploymentName);
    }

    public IEnumerable<ChatModelInfo> GetAvailableModels()
    {
        if (!IsAvailable())
        {
            return Enumerable.Empty<ChatModelInfo>();
        }

        // Return models from the deployments list in configuration
        // If no deployments are configured, fall back to the legacy single deployment
        var deployments = _settings.AzureOpenAI.Deployments;
        if (deployments == null || !deployments.Any())
        {
            // Fallback to legacy single deployment configuration
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
                Capabilities = new List<string> { "chat", "json-schema", "story-generation" }
            };
            return new List<ChatModelInfo> { model };
        }

        var configuredName = _settings.AzureOpenAI.DeploymentName;

        // If a specific deployment is configured, prefer returning only that one
        if (!string.IsNullOrWhiteSpace(configuredName))
        {
            var selected = deployments.FirstOrDefault(d => string.Equals(d.Name, configuredName, StringComparison.OrdinalIgnoreCase));
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
                        Capabilities = selected.Capabilities ?? new List<string> { "chat", "story-generation" }
                    }
                };
            }

            // Not found in list: return a synthetic single entry
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
                    Capabilities = new List<string> { "chat", "json-schema", "story-generation" }
                }
            };
        }

        // No specific configured deployment: return all configured entries
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
            Capabilities = deployment.Capabilities ?? new List<string> { "chat", "story-generation" }
        });
    }

    private static string GetDisplayNameForDeployment(string deploymentName)
    {
        // Convert deployment name to a more user-friendly display name
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

    public async Task<ChatCompletionResponse> CompleteAsync(ChatCompletionRequest request, CancellationToken cancellationToken = default)
    {
        if (!IsAvailable()) return CreateErrorResponse("Azure OpenAI service is not properly configured");

        try
        {
            // Determine which deployment to use
            var deploymentName = ResolveDeploymentName(request);

            // Resolve the endpoint for the deployment
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
            var isIncomplete = finishReason == "Length"; // OpenAI uses PascalCase for FinishReason enum ToString() or specific strings

            return new ChatCompletionResponse
            {
                Content = cleanContent ?? string.Empty,
                Model = deploymentName,
                Provider = ProviderName,
                Usage = response?.Usage != null ? new ChatCompletionUsage
                {
                    PromptTokens = response.Usage.InputTokenCount,
                    CompletionTokens = response.Usage.OutputTokenCount,
                    TotalTokens = response.Usage.TotalTokenCount
                } : null,
                Success = true,
                FinishReason = finishReason,
                IsIncomplete = isIncomplete
            };

            string? Sanitize(string? input)
            {
                if (input == null) return null;

                var sb = new StringBuilder(input.Length);
                foreach (var ch in input)
                {
                    var cat = char.GetUnicodeCategory(ch);
                    var isControl = cat == UnicodeCategory.Control;

                    // Keep common whitespace controls, drop everything else
                    if (isControl && ch != '\n' && ch != '\r' && ch != '\t')
                        continue;

                    sb.Append(ch);
                }

                return sb.ToString();
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calling Azure OpenAI API");
            return CreateErrorResponse($"Azure OpenAI service error: {ex.Message}");
        }
    }

    private string ResolveDeploymentName(ChatCompletionRequest request)
    {
        // Priority order:
        // 1. Deployment name specified in request
        // 2. Stored deployment name
        // 3. Default deployment name from settings

        if (!string.IsNullOrEmpty(request.Model))
        {
            _deploymentNameOrModelId = request.Model;
            return request.Model;
        }

        if (!string.IsNullOrWhiteSpace(_deploymentNameOrModelId)) return _deploymentNameOrModelId;

        _deploymentNameOrModelId = _settings.AzureOpenAI.DeploymentName;
        return _settings.AzureOpenAI.DeploymentName;
    }

    private string ResolveEndpoint(string deploymentName)
    {
        // Priority order:
        // 1. Endpoint configured for the specific deployment
        // 2. Default endpoint from settings

        if (!string.IsNullOrWhiteSpace(deploymentName) && _settings.AzureOpenAI.Deployments != null)
        {
            var deployment = _settings.AzureOpenAI.Deployments.FirstOrDefault(d => d.Name == deploymentName);
            if (deployment != null && !string.IsNullOrWhiteSpace(deployment.Endpoint))
            {
                return deployment.Endpoint;
            }
        }

        return _settings.AzureOpenAI.Endpoint;
    }

    // Exposed for unit testing to validate option construction when a JsonSchemaFormat is provided.
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
                logger.LogWarning(ex, "Failed to configure JSON schema response format. Falling back to default response format.");
                return null;
            }
        }

        return null;
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
