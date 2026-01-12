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
        services.AddSingleton(Options.Create(foundryConfig));

        services.AddSingleton<FoundryAgentClient>(sp =>
        {
            var logger = sp.GetRequiredService<ILogger<FoundryAgentClient>>();
            var client = new FoundryAgentClient(logger);

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
                        ?? throw new InvalidOperationException("AISearch endpoint is required. Set FoundryAgent:AISearch:Endpoint"),
                    ApiKey = foundryConfig.AISearch?.ApiKey
                        ?? foundryConfig.ApiKey  // Backward compatibility
                        ?? throw new InvalidOperationException("AISearch API key is required. Set FoundryAgent:AISearch:ApiKey"),
                    IndexName = foundryConfig.AISearch?.IndexName
                        ?? throw new InvalidOperationException("AISearch index name is required. Set FoundryAgent:AISearch:IndexName"),
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

                // Prefer new agent-specific config, fall back to legacy config
                #pragma warning disable CS0618 // Type or member is obsolete
                var fileSearchConfig = new FileSearchKnowledgeProvider.FileSearchConfiguration
                {
                    VectorStoresByAgentAndAge = foundryConfig.FileSearch?.VectorStoresByAgentAndAge
                        ?? new Dictionary<string, Dictionary<string, string>>(),
                    VectorStoresByAgeGroup = foundryConfig.FileSearch?.VectorStoresByAgeGroup
                        ?? new Dictionary<string, string>(),
                    MaxFiles = foundryConfig.FileSearch?.MaxFiles,
                    MaxTokens = foundryConfig.FileSearch?.MaxTokens
                };
                #pragma warning restore CS0618 // Type or member is obsolete

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

            var cosmosClient = new Microsoft.Azure.Cosmos.Fluent.CosmosClientBuilder(
                cosmosConfig.Endpoint,
                cosmosConfig.ApiKey)
                .WithSystemTextJsonSerializerOptions(new System.Text.Json.JsonSerializerOptions
                {
                    PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase,
                    DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull
                })
                .Build();

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
