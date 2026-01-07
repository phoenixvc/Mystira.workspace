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

                // Use new nested config if available, otherwise fall back to legacy config
                var aiSearchConfig = new AISearchKnowledgeProvider.AISearchConfiguration
                {
                    Endpoint = foundryConfig.AISearch?.Endpoint
                        ?? foundryConfig.Endpoint  // Backward compatibility
                        ?? throw new InvalidOperationException("AISearch endpoint is required. Set FoundryAgent:AISearch:Endpoint"),
                    ApiKey = foundryConfig.AISearch?.ApiKey
                        ?? foundryConfig.ApiKey  // Backward compatibility
                        ?? throw new InvalidOperationException("AISearch API key is required. Set FoundryAgent:AISearch:ApiKey"),
                    IndexName = foundryConfig.AISearch?.IndexName
#pragma warning disable CS0618 // Type or member is obsolete
                        ?? foundryConfig.SearchIndexName  // Backward compatibility (deprecated)
#pragma warning restore CS0618
                        ?? "mystira-instructions",
                    ContentFieldName = foundryConfig.AISearch?.ContentFieldName,
                    AgeGroupFieldName = foundryConfig.AISearch?.AgeGroupFieldName ?? "age_group",
                    TitleFieldName = foundryConfig.AISearch?.ContentFieldName
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

                // Use new nested config if available, otherwise fall back to legacy config
                Dictionary<string, string>? vectorStoresByAgeGroup = foundryConfig.FileSearch?.VectorStoresByAgeGroup;

                // Backward compatibility: check legacy config if new config not present
                if (vectorStoresByAgeGroup == null || vectorStoresByAgeGroup.Count == 0)
                {
#pragma warning disable CS0618 // Type or member is obsolete
                    vectorStoresByAgeGroup = foundryConfig.VectorStoresByAgeGroup;
#pragma warning restore CS0618
                }

                var fileSearchConfig = new FileSearchKnowledgeProvider.FileSearchConfiguration
                {
                    VectorStoresByAgeGroup = vectorStoresByAgeGroup ?? new Dictionary<string, string>(),
                    MaxFiles = foundryConfig.FileSearch?.MaxFiles,
                    MaxTokens = foundryConfig.FileSearch?.MaxTokens
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
