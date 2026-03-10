using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Mystira.StoryGenerator.Application.Infrastructure.Agents;
using Mystira.StoryGenerator.Application.Services;
using Mystira.StoryGenerator.Application.Services.Prompting;
using Mystira.StoryGenerator.Contracts.Configuration;
using Mystira.StoryGenerator.Domain.Agents;
using Mystira.StoryGenerator.Infrastructure.Agents;

namespace Mystira.StoryGenerator.Application;

public static class FoundryServiceCollectionExtensions
{
    public static IServiceCollection AddFoundryAgentServices(
        this IServiceCollection services)
    {
        services.AddSingleton<IFoundryAgentClient>(sp =>
        {
            var logger = sp.GetRequiredService<ILogger<FoundryAgentClient>>();
            var foundryConfig = sp.GetRequiredService<IOptions<FoundryAgentConfig>>().Value;
            var client = new FoundryAgentClient(logger);

            // Validate agent IDs are properly configured (not placeholder values)
            FoundryAgentConfigValidator.ValidateAgentIds(foundryConfig);

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

        services.AddScoped<IKnowledgeProvider>(sp =>
        {
            var client = sp.GetRequiredService<IFoundryAgentClient>();
            var foundryConfig = sp.GetRequiredService<IOptions<FoundryAgentConfig>>().Value;

            if (foundryConfig.KnowledgeMode.Equals("AISearch", StringComparison.OrdinalIgnoreCase))
            {
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
            }
            else
            {
                var logger = sp.GetRequiredService<ILogger<FileSearchKnowledgeProvider>>();

                // Use the new FileSearchConfig
                var fileSearchConfig = foundryConfig.FileSearch ?? new FileSearchConfig();

                return new FileSearchKnowledgeProvider(client, fileSearchConfig, logger);
            }
        });

        services.AddSingleton<IProjectGuidelinesService, ProjectGuidelinesService>();
        services.AddScoped<IStoryMediaProcessor, StoryMediaProcessor>();
        services.AddScoped<IPromptGenerator, PromptGenerator>();
        services.AddSingleton<IStorySchemaValidator, StorySchemaValidator>();

        services.AddScoped<IStorySessionRepository>(sp =>
        {
            var logger = sp.GetRequiredService<ILogger<CosmosStorySessionRepository>>();
            var options = sp.GetRequiredService<IOptions<CosmosDbConfig>>();
            var cosmosConfig = options.Value;

            if (string.IsNullOrEmpty(cosmosConfig.Endpoint))
            {
                logger.LogError("Cosmos DB endpoint is not configured. Check CosmosDb:Endpoint setting.");
                throw new InvalidOperationException("Cosmos DB endpoint is required.");
            }

            var rawKey = cosmosConfig.ApiKey ?? string.Empty;
            // Remove all whitespace including hidden non-breaking spaces
            var apiKey = new string(rawKey.Where(c => !char.IsWhiteSpace(c)).ToArray());

            if (string.IsNullOrEmpty(apiKey))
            {
                logger.LogError("Cosmos DB API Key is not configured. Check CosmosDb:ApiKey setting. Current Environment: {Environment}", Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT"));
                throw new InvalidOperationException($"Cosmos DB API Key is required. Environment: {Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT")}");
            }

            // Safe diagnostic logging (don't log the full key)
            var keyLength = apiKey.Length;
            var prefix = apiKey.Length >= 3 ? apiKey.Substring(0, 3) : "...";
            var suffix = apiKey.Length >= 3 ? apiKey.Substring(apiKey.Length - 3) : "...";
            logger.LogInformation("Initializing CosmosClient. Key Length: {Length}, Prefix: {Prefix}, Suffix: {Suffix}", keyLength, prefix, suffix);

            var cosmosClient = new Microsoft.Azure.Cosmos.Fluent.CosmosClientBuilder(
                cosmosConfig.Endpoint,
                apiKey)
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
        this IServiceCollection services)
    {
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
