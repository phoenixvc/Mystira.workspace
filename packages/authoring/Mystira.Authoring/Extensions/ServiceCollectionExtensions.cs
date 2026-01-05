using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Mystira.Authoring.Abstractions.Services;
using Mystira.Authoring.Graph;
using Mystira.Authoring.Services;

namespace Mystira.Authoring.Extensions;

/// <summary>
/// Extension methods for configuring authoring services in dependency injection.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Adds Mystira Authoring services to the service collection.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="configuration">The configuration root.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddMystiraAuthoring(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // Register graph utilities
        services.AddSingleton<ScenarioGraphBuilder>();

        // Register services
        services.AddScoped<IConsistencyEvaluationService, ConsistencyEvaluationService>();

        // Note: Command handlers are discovered by Wolverine via convention
        // No explicit registration needed

        return services;
    }
}
