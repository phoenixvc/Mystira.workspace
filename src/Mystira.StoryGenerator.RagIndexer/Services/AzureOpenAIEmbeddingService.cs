using System.ClientModel;
using Azure.AI.OpenAI;
using Mystira.StoryGenerator.RagIndexer.Configuration;

namespace Mystira.StoryGenerator.RagIndexer.Services;

public class AzureOpenAIEmbeddingService
{
    private readonly AzureOpenAIClient _client;
    private readonly string _deploymentName;

    public AzureOpenAIEmbeddingService(AzureOpenAIEmbeddingSettings settings)
    {
        _client = new AzureOpenAIClient(
            new Uri(settings.Endpoint),
            new ApiKeyCredential(settings.ApiKey));
        _deploymentName = settings.DeploymentName;
    }

    public async Task<IReadOnlyList<float>> GenerateEmbeddingAsync(string text)
    {
        try
        {
            var embeddingClient = _client.GetEmbeddingClient(_deploymentName);
            var response = await embeddingClient.GenerateEmbeddingAsync(text);
            
            return response.Value.ToFloats().ToArray();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error generating embedding: {ex.Message}");
            throw;
        }
    }
}