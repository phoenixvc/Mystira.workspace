using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.DependencyInjection;
using Mystira.Shared.Authorization;

namespace Mystira.Shared.Extensions;

/// <summary>
/// Extension methods for configuring Mystira authorization.
/// </summary>
public static class AuthorizationExtensions
{
    /// <summary>
    /// Adds Mystira authorization services with permission-based policies.
    /// </summary>
    /// <param name="services">The service collection</param>
    /// <returns>The service collection for chaining</returns>
    public static IServiceCollection AddMystiraAuthorization(this IServiceCollection services)
    {
        services.AddAuthorization(options =>
        {
            // Add permission-based policies
            AddPermissionPolicies(options);

            // Add role-based policies
            AddRolePolicies(options);

            // Default policy requires authenticated user
            options.DefaultPolicy = new AuthorizationPolicyBuilder()
                .RequireAuthenticatedUser()
                .Build();

            // Fallback policy (for endpoints without [Authorize])
            options.FallbackPolicy = null; // Allow anonymous by default
        });

        // Register authorization handlers
        services.AddScoped<IAuthorizationHandler, PermissionHandler>();

        return services;
    }

    private static void AddPermissionPolicies(AuthorizationOptions options)
    {
        // Dynamically create policies for all permissions
        var permissionFields = typeof(Permissions).GetFields()
            .Where(f => f.IsLiteral && !f.IsInitOnly);

        foreach (var field in permissionFields)
        {
            var permission = (string)field.GetValue(null)!;
            var policyName = $"{RequirePermissionAttribute.PolicyPrefix}{permission}";

            options.AddPolicy(policyName, policy =>
                policy.Requirements.Add(new PermissionRequirement(permission)));
        }
    }

    private static void AddRolePolicies(AuthorizationOptions options)
    {
        // Admin-only policy
        options.AddPolicy("AdminOnly", policy =>
            policy.RequireRole(Roles.Admin));

        // Moderator or above policy
        options.AddPolicy("ModeratorOrAbove", policy =>
            policy.RequireRole(Roles.Admin, Roles.Moderator));

        // Creator or above policy
        options.AddPolicy("CreatorOrAbove", policy =>
            policy.RequireRole(Roles.Admin, Roles.Moderator, Roles.Creator));

        // Service account policy
        options.AddPolicy("ServiceOnly", policy =>
            policy.RequireRole(Roles.Service));
    }
}
