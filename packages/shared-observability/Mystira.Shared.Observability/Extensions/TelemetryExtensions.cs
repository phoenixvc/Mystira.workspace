using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Mystira.Shared.Middleware;

namespace Mystira.Shared.Extensions;

/// <summary>
/// Extension methods for configuring Mystira telemetry.
/// </summary>
public static class TelemetryExtensions
{
    /// <summary>
    /// Adds Mystira telemetry services.
    /// </summary>
    /// <param name="services">The service collection</param>
    /// <param name="configuration">The configuration</param>
    /// <returns>The service collection for chaining</returns>
    public static IServiceCollection AddMystiraTelemetry(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.Configure<TelemetryOptions>(
            configuration.GetSection(TelemetryOptions.SectionName));

        return services;
    }

    /// <summary>
    /// Adds Mystira telemetry middleware to the pipeline.
    /// </summary>
    /// <param name="app">The application builder</param>
    /// <returns>The application builder for chaining</returns>
    public static IApplicationBuilder UseMystiraTelemetry(this IApplicationBuilder app)
    {
        app.UseMiddleware<TelemetryMiddleware>();
        return app;
    }
}
