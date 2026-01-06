using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Mystira.StoryGenerator.Application.Infrastructure.Agents;
using Mystira.StoryGenerator.Application.Services.Prompting;
using Mystira.StoryGenerator.Contracts.Configuration;
using Mystira.StoryGenerator.Domain.Agents;
using Mystira.StoryGenerator.Infrastructure.Agents;

namespace Mystira.StoryGenerator.Application;

/// <summary>
/// Extension methods for registering Azure AI Foundry services.
/// </summary>
public static class FoundryServiceCollectionExtensions
{
    /// <summary>
    /// Adds Azure AI Foundry Agent services to the DI container.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="configuration">The Foundry configuration.</param>
    /// <returns>The service collection for method chaining.</returns>
    public static IServiceCollection AddFoundryAgentServices(
        this IServiceCollection services,
        FoundryAgentConfig configuration)
    {
        // Register configuration as singleton
        services.AddSingleton(configuration);

        // Register Foundry Agent Client as singleton
        services.AddSingleton<FoundryAgentClient>(sp =>
        {
            var logger = sp.GetRequiredService<ILogger<FoundryAgentClient>>();
            var client = new FoundryAgentClient();

            var clientConfig = new FoundryAgentClientConfig
            {
                Endpoint = configuration.Endpoint,
                ApiKey = configuration.ApiKey,
                ProjectId = configuration.ProjectId
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

        // Register knowledge provider based on configuration
        if (configuration.KnowledgeMode.Equals("AISearch", StringComparison.OrdinalIgnoreCase))
        {
            services.AddScoped<IKnowledgeProvider>(sp =>
            {
                var client = sp.GetRequiredService<FoundryAgentClient>();
                var logger = sp.GetRequiredService<ILogger<AISearchKnowledgeProvider>>();

                var aiSearchConfig = new AISearchKnowledgeProvider.AISearchConfiguration
                {
                    Endpoint = configuration.Endpoint,
                    ApiKey = configuration.ApiKey,
                    IndexName = configuration.SearchIndexName ?? "mystira-instructions"
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
                    VectorStoreName = configuration.VectorStoreName ?? "mystira-story-knowledge"
                };

                return new FileSearchKnowledgeProvider(client, fileSearchConfig, logger);
            });
        }

        services.AddSingleton<IProjectGuidelinesService, ProjectGuidelinesService>();
        services.AddScoped<IPromptGenerator, PromptGenerator>();
        services.AddSingleton<StorySchemaValidator>();

        // Register story session repository
        services.AddScoped<IStorySessionRepository>(sp =>
        {
            var logger = sp.GetRequiredService<ILogger<CosmosStorySessionRepository>>();
            var cosmosConfig = sp.GetRequiredService<IOptions<CosmosDbConfig>>().Value;

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

    /// <summary>
    /// Adds Cosmos DB configuration binding.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="configuration">The configuration section name.</param>
    /// <returns>The service collection for method chaining.</returns>
    public static IServiceCollection AddCosmosDbConfiguration(
        this IServiceCollection services,
        string configurationSectionName = "CosmosDb")
    {
        services.AddOptions<CosmosDbConfig>()
            .Bind(configurationSectionName)
            .ValidateDataAnnotations()
            .ValidateOnStart();

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
