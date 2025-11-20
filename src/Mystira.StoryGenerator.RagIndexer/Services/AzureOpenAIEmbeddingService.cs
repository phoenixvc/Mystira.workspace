using System.ClientModel;
using Azure.AI.OpenAI;
using Mystira.StoryGenerator.RagIndexer.Configuration;
using Mystira.StoryGenerator.RagIndexer.Interfaces;

namespace Mystira.StoryGenerator.RagIndexer.Services;

public class AzureOpenAIEmbeddingService : IAzureOpenAIEmbeddingService
{
    private readonly AzureOpenAIClient _client;
    private readonly string _deploymentName;
    private readonly ILoggerService _logger;
    private readonly IRetryPolicyService _retryPolicy;

    public AzureOpenAIEmbeddingService(
        AzureOpenAIEmbeddingSettings settings,
        ILoggerService logger,
        IRetryPolicyService retryPolicy)
    {
        _client = new AzureOpenAIClient(
            new Uri(settings.Endpoint),
            new ApiKeyCredential(settings.ApiKey));
        _deploymentName = settings.DeploymentName;
        _logger = logger;
        _retryPolicy = retryPolicy;
    }

    public async Task<IReadOnlyList<float>> GenerateEmbeddingAsync(string text)
    {
        Func<Task<IReadOnlyList<float>>> operation = async () =>
        {
            var embeddingClient = _client.GetEmbeddingClient(_deploymentName);
            var response = await embeddingClient.GenerateEmbeddingAsync(text);
            
            var embedding = response.Value.ToFloats().ToArray();
            return embedding;
        };

        var result = await _retryPolicy.ExecuteWithRetryAsync(operation, "GenerateEmbeddingAsync", maxRetries: 5);
        _logger.LogInfo($"Generated embedding with {result.Count} dimensions");
        
        return result;
    }
}