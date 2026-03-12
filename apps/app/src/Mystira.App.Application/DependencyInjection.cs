using Microsoft.Extensions.DependencyInjection;
using Mystira.Core;

namespace Mystira.App.Application;

/// <summary>
/// Extension methods for registering app-specific Application layer services.
/// Delegates shared registrations to Mystira.Core.
/// </summary>
public static class DependencyInjection
{
    /// <summary>
    /// Registers all Application layer services. Calls Core registration
    /// and adds any app-specific services.
    /// </summary>
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        // Register all shared Core services (validators, use cases, application services)
        services.AddCoreApplicationServices();

        return services;
    }
}
