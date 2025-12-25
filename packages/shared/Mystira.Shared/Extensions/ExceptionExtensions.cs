using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Mystira.Shared.Exceptions;

namespace Mystira.Shared.Extensions;

/// <summary>
/// Extension methods for exception handling registration.
/// </summary>
public static class ExceptionExtensions
{
    /// <summary>
    /// Adds the global exception handler to the service collection.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddMystiraExceptionHandling(this IServiceCollection services)
    {
        services.AddExceptionHandler<GlobalExceptionHandler>();
        services.AddProblemDetails();

        return services;
    }

    /// <summary>
    /// Uses the global exception handler middleware.
    /// Should be called early in the pipeline.
    /// </summary>
    /// <param name="app">The application builder.</param>
    /// <returns>The application builder for chaining.</returns>
    public static IApplicationBuilder UseMystiraExceptionHandling(this IApplicationBuilder app)
    {
        app.UseExceptionHandler();

        return app;
    }
}
