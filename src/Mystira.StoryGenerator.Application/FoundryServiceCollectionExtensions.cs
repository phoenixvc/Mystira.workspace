using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Mystira.StoryGenerator.Application.Infrastructure.Agents;
using Mystira.StoryGenerator.Application.Services.Prompting;
using Mystira.StoryGenerator.Contracts.Configuration;
using Mystira.StoryGenerator.Domain.Agents;
using Mystira.StoryGenerator.Infrastructure.Agents;

namespace Mystira.StoryGenerator.Application;

public static class FoundryServiceCollectionExtensions
{
    public static IServiceCollection AddFoundryAgentServices(
        this IServiceCollection services,
        FoundryAgentConfig foundryConfig)
    {
        services.AddSingleton(foundryConfig);

        services.AddSingleton<FoundryAgentClient>(sp =>
        {
            var logger = sp.GetRequiredService<ILogger<FoundryAgentClient>>();
            var client = FoundryAgentClient.GetInstance(logger);

            var clientConfig = new FoundryAgentClientConfig
            {
                Endpoint = foundryConfig.Endpoint,
                ApiKey = foundryConfig.ApiKey,
                ProjectId = foundryConfig.ProjectId
            };

            try
            {
                client.Initialize(clientConfig);
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex, "Failed to initialize FoundryAgentClient. Services will be available but may fail at runtime.");
            }

            return client;
        });

        if (foundryConfig.KnowledgeMode.Equals("AISearch", StringComparison.OrdinalIgnoreCase))
        {
            services.AddScoped<IKnowledgeProvider>(sp =>
            {
                var client = sp.GetRequiredService<FoundryAgentClient>();
                var logger = sp.GetRequiredService<ILogger<AISearchKnowledgeProvider>>();

                var aiSearchConfig = new AISearchKnowledgeProvider.AISearchConfiguration
                {
                    Endpoint = foundryConfig.Endpoint,
                    ApiKey = foundryConfig.ApiKey,
                    IndexName = foundryConfig.SearchIndexName ?? "mystira-instructions"
                };

                return new AISearchKnowledgeProvider(client, aiSearchConfig, logger);
            });
        }
        else
        {
            services.AddScoped<IKnowledgeProvider>(sp =>
            {
                var client = sp.GetRequiredService<FoundryAgentClient>();
                var logger = sp.GetRequiredService<ILogger<FileSearchKnowledgeProvider>>();

                var fileSearchConfig = new FileSearchKnowledgeProvider.FileSearchConfiguration
                {
                    VectorStoreName = foundryConfig.VectorStoreName ?? "mystira-story-knowledge",
                    VectorStoresByAgeGroup = foundryConfig.VectorStoresByAgeGroup
                };

                return new FileSearchKnowledgeProvider(client, fileSearchConfig, logger);
            });
        }

        services.AddSingleton<IProjectGuidelinesService, ProjectGuidelinesService>();
        services.AddScoped<IPromptGenerator, PromptGenerator>();
        services.AddSingleton<StorySchemaValidator>();

        services.AddScoped<IStorySessionRepository>(sp =>
        {
            var logger = sp.GetRequiredService<ILogger<CosmosStorySessionRepository>>();
            var options = sp.GetRequiredService<IOptions<CosmosDbConfig>>();
            var cosmosConfig = options.Value;

            var cosmosClient = new Microsoft.Azure.Cosmos.CosmosClient(
                cosmosConfig.Endpoint,
                cosmosConfig.ApiKey);

            return new CosmosStorySessionRepository(
                cosmosClient,
                cosmosConfig.DatabaseId,
                cosmosConfig.ContainerId,
                logger);
        });

        return services;
    }

    public static IServiceCollection AddCosmosDbConfiguration(
        this IServiceCollection services,
        CosmosDbConfig cosmosConfig)
    {
        services.AddSingleton(Options.Create(cosmosConfig));
        return services;
    }
}

/// <summary>
/// Cosmos DB configuration for story session storage.
/// </summary>
public class CosmosDbConfig
{
    public const string SectionName = "CosmosDb";

    public string Endpoint { get; set; } = string.Empty;
    public string ApiKey { get; set; } = string.Empty;
    public string DatabaseId { get; set; } = "MystiraStoryGenerator";
    public string ContainerId { get; set; } = "StorySessions";
}
