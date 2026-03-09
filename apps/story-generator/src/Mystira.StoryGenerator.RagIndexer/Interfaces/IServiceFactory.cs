using Mystira.StoryGenerator.RagIndexer.Configuration;

namespace Mystira.StoryGenerator.RagIndexer.Interfaces;

public interface IServiceFactory
{
    IAzureAISearchService CreateAzureAISearchService(AzureAISearchSettings settings);
    IAzureOpenAIEmbeddingService CreateAzureOpenAIEmbeddingService(AzureOpenAIEmbeddingSettings settings);
    IRagIndexingService CreateRagIndexingService(IAzureAISearchService searchService, IAzureOpenAIEmbeddingService embeddingService);
}