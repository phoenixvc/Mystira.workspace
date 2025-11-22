using System.ClientModel;
using Azure.AI.OpenAI;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Logging;
using Mystira.StoryGenerator.Contracts.Chat;
using Mystira.StoryGenerator.Contracts.Configuration;
using Mystira.StoryGenerator.Domain.Services;
using Mystira.StoryGenerator.Llm.Extensions;
using OpenAI.Chat;

namespace Mystira.StoryGenerator.Llm.Services.LLM;

/// <summary>
/// Azure OpenAI implementation of ILLMService
/// </summary>
public class AzureOpenAIService : ILLMService
{
    private readonly AiSettings _settings;
    private readonly ILogger<AzureOpenAIService> _logger;

    public string ProviderName => "azure-openai";

    public AzureOpenAIService(IOptions<AiSettings> options, ILogger<AzureOpenAIService> logger)
    {
        _settings = options.Value;
        _logger = logger;
    }

    public bool IsAvailable()
    {
        return !string.IsNullOrWhiteSpace(_settings.AzureOpenAI.ApiKey) &&
               !string.IsNullOrWhiteSpace(_settings.AzureOpenAI.Endpoint) &&
               !string.IsNullOrWhiteSpace(_settings.AzureOpenAI.DeploymentName);
    }

    public async Task<ChatCompletionResponse> CompleteAsync(ChatCompletionRequest request, CancellationToken cancellationToken = default)
    {
        if (!IsAvailable()) return CreateErrorResponse("Azure OpenAI service is not properly configured");

        try
        {
            var azureClient = new AzureOpenAIClient(
                new Uri(_settings.AzureOpenAI.Endpoint),
                new ApiKeyCredential(_settings.AzureOpenAI.ApiKey),
                new AzureOpenAIClientOptions
                {
                    NetworkTimeout = new TimeSpan(0, 0, 3, 0)
                });
            var chatClient = azureClient.GetChatClient(_settings.AzureOpenAI.DeploymentName);

            var messages = request.ToOpenAiChatMessages();

            var options = BuildOptions(request, _logger);

            var azureResponse = await chatClient.CompleteChatAsync(
                messages,
                options: options,
                cancellationToken: cancellationToken);
            var response = azureResponse.Value;

            return new ChatCompletionResponse
            {
                Content = response?.Content?.FirstOrDefault()?.Text ?? string.Empty,
                Model = response?.Model ?? _settings.AzureOpenAI.DeploymentName,
                Provider = ProviderName,
                Usage = response?.Usage != null ? new ChatCompletionUsage
                {
                    PromptTokens = response.Usage.InputTokenCount,
                    CompletionTokens = response.Usage.OutputTokenCount,
                    TotalTokens = response.Usage.TotalTokenCount
                } : null,
                Success = true
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calling Azure OpenAI API");
            return CreateErrorResponse($"Azure OpenAI service error: {ex.Message}");
        }
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
