using Microsoft.Extensions.DependencyInjection;
using OpenTelemetry.Resources;

namespace Mystira.Shared.Telemetry;

/// <summary>
/// Extension methods for configuring cloud role name via OpenTelemetry resource attributes.
/// In Application Insights 3.x, cloud role name is set through OpenTelemetry resource
/// attributes (service.name, service.namespace, service.instance.id) instead of the
/// removed ITelemetryInitializer.
/// </summary>
public static class CloudRoleNameConfiguration
{
    /// <summary>
    /// Configures OpenTelemetry resource attributes for cloud role identification.
    /// Sets service.name (used as Cloud Role Name in Application Map) and
    /// service.instance.id (used as Cloud Role Instance).
    /// </summary>
    /// <param name="builder">The resource builder to configure.</param>
    /// <param name="roleName">The name of the service (e.g., "Mystira.App.Api")</param>
    /// <param name="environment">The environment name (e.g., "Development", "Production")</param>
    /// <returns>The resource builder for chaining.</returns>
    public static ResourceBuilder AddCloudRoleAttributes(
        this ResourceBuilder builder,
        string roleName,
        string environment)
    {
        ArgumentNullException.ThrowIfNull(roleName);

        return builder.AddAttributes(new Dictionary<string, object>
        {
            ["service.name"] = roleName,
            ["service.instance.id"] = $"{roleName}-{environment ?? "Unknown"}",
            ["deployment.environment"] = environment ?? "Unknown"
        });
    }
}
