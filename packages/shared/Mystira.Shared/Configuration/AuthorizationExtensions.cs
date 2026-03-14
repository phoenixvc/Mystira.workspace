using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.DependencyInjection;

namespace Mystira.Shared.Configuration;

/// <summary>
/// Extension methods for adding Mystira authorization policies.
/// </summary>
public static class AuthorizationExtensions
{
    /// <summary>
    /// Adds Mystira authorization policies to the authorization options.
    /// </summary>
    public static AuthorizationOptions AddMystiraAuthorizationPolicies(this AuthorizationOptions options)
    {
        options.AddPolicy("AdminOnly", policy =>
            policy.RequireRole("Admin", "SuperAdmin"));

        options.AddPolicy("CanModerate", policy =>
            policy.RequireRole("Moderator", "Admin", "SuperAdmin"));

        options.AddPolicy("ReadOnly", policy =>
            policy.RequireRole("Viewer", "Moderator", "Admin", "SuperAdmin"));

        options.AddPolicy("SuperAdminOnly", policy =>
            policy.RequireRole("SuperAdmin"));

        return options;
    }

    /// <summary>
    /// Adds Mystira authorization policies to the service collection.
    /// </summary>
    public static IServiceCollection AddMystiraAuthorizationPolicies(this IServiceCollection services)
    {
        services.AddAuthorization(options =>
        {
            options.AddMystiraAuthorizationPolicies();
        });

        return services;
    }
}
