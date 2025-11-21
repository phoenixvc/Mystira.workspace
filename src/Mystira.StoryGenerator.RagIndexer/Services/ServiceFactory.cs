using Mystira.StoryGenerator.RagIndexer.Configuration;
using Mystira.StoryGenerator.RagIndexer.Interfaces;

namespace Mystira.StoryGenerator.RagIndexer.Services;

public class ServiceFactory : IServiceFactory
{
    private readonly ILoggerService _logger;
    private readonly IRetryPolicyService _retryPolicy;

    public ServiceFactory(ILoggerService logger, IRetryPolicyService retryPolicy)
    {
        _logger = logger;
        _retryPolicy = retryPolicy;
    }

    public IAzureAISearchService CreateAzureAISearchService(AzureAISearchSettings settings)
    {
        return new AzureAISearchService(settings, _logger, _retryPolicy);
    }

    public IAzureOpenAIEmbeddingService CreateAzureOpenAIEmbeddingService(AzureOpenAIEmbeddingSettings settings)
    {
        return new AzureOpenAIEmbeddingService(settings, _logger, _retryPolicy);
    }

    public IRagIndexingService CreateRagIndexingService(IAzureAISearchService searchService, IAzureOpenAIEmbeddingService embeddingService)
    {
        return new RagIndexingService(searchService, embeddingService, _logger);
    }
}