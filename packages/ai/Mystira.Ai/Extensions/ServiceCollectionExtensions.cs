using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Mystira.Ai.Abstractions;
using Mystira.Ai.Configuration;
using Mystira.Ai.Providers;

namespace Mystira.Ai.Extensions;

/// <summary>
/// Extension methods for configuring AI services in dependency injection.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Adds Mystira AI services to the service collection.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="configuration">The configuration root.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddMystiraAi(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // Bind configuration
        services.Configure<AiSettings>(configuration.GetSection(AiSettings.SectionName));

        // Register LLM providers
        services.AddSingleton<ILLMService, AzureOpenAIService>();
        services.AddSingleton<ILLMService, AnthropicAIService>();

        // Register factory
        services.AddSingleton<ILlmServiceFactory, LLMServiceFactory>();

        return services;
    }

    /// <summary>
    /// Adds Mystira AI services with custom configuration action.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="configureOptions">Action to configure AI settings.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddMystiraAi(
        this IServiceCollection services,
        Action<AiSettings> configureOptions)
    {
        // Configure options
        services.Configure(configureOptions);

        // Register LLM providers
        services.AddSingleton<ILLMService, AzureOpenAIService>();
        services.AddSingleton<ILLMService, AnthropicAIService>();

        // Register factory
        services.AddSingleton<ILlmServiceFactory, LLMServiceFactory>();

        return services;
    }
}
