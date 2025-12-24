using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace Mystira.Shared.Health;

/// <summary>
/// Extension methods for registering Mystira health checks.
/// </summary>
public static class HealthCheckExtensions
{
    /// <summary>
    /// Adds Redis health check.
    /// </summary>
    /// <param name="builder">The health checks builder.</param>
    /// <param name="name">The name of the health check.</param>
    /// <param name="failureStatus">The failure status to report.</param>
    /// <param name="tags">Tags for the health check.</param>
    /// <returns>The health checks builder for chaining.</returns>
    public static IHealthChecksBuilder AddRedisHealthCheck(
        this IHealthChecksBuilder builder,
        string name = "redis",
        HealthStatus? failureStatus = null,
        IEnumerable<string>? tags = null)
    {
        return builder.AddCheck<RedisHealthCheck>(
            name,
            failureStatus,
            tags ?? new[] { "ready", "cache" });
    }

    /// <summary>
    /// Adds database health check for the specified DbContext.
    /// </summary>
    /// <typeparam name="TContext">The DbContext type.</typeparam>
    /// <param name="builder">The health checks builder.</param>
    /// <param name="name">The name of the health check.</param>
    /// <param name="failureStatus">The failure status to report.</param>
    /// <param name="tags">Tags for the health check.</param>
    /// <returns>The health checks builder for chaining.</returns>
    public static IHealthChecksBuilder AddDatabaseHealthCheck<TContext>(
        this IHealthChecksBuilder builder,
        string? name = null,
        HealthStatus? failureStatus = null,
        IEnumerable<string>? tags = null)
        where TContext : DbContext
    {
        return builder.AddCheck<DatabaseHealthCheck<TContext>>(
            name ?? $"database-{typeof(TContext).Name.ToLowerInvariant()}",
            failureStatus,
            tags ?? new[] { "ready", "database" });
    }

    /// <summary>
    /// Adds Wolverine messaging health check.
    /// </summary>
    /// <param name="builder">The health checks builder.</param>
    /// <param name="name">The name of the health check.</param>
    /// <param name="failureStatus">The failure status to report.</param>
    /// <param name="tags">Tags for the health check.</param>
    /// <returns>The health checks builder for chaining.</returns>
    public static IHealthChecksBuilder AddWolverineHealthCheck(
        this IHealthChecksBuilder builder,
        string name = "wolverine",
        HealthStatus? failureStatus = null,
        IEnumerable<string>? tags = null)
    {
        return builder.AddCheck<WolverineHealthCheck>(
            name,
            failureStatus,
            tags ?? new[] { "ready", "messaging" });
    }

    /// <summary>
    /// Adds all Mystira infrastructure health checks.
    /// Includes Redis (if configured), Wolverine (if enabled).
    /// </summary>
    /// <param name="builder">The health checks builder.</param>
    /// <returns>The health checks builder for chaining.</returns>
    public static IHealthChecksBuilder AddMystiraHealthChecks(this IHealthChecksBuilder builder)
    {
        return builder
            .AddRedisHealthCheck()
            .AddWolverineHealthCheck();
    }

    /// <summary>
    /// Adds all Mystira infrastructure health checks including database.
    /// </summary>
    /// <typeparam name="TContext">The DbContext type.</typeparam>
    /// <param name="builder">The health checks builder.</param>
    /// <returns>The health checks builder for chaining.</returns>
    public static IHealthChecksBuilder AddMystiraHealthChecks<TContext>(this IHealthChecksBuilder builder)
        where TContext : DbContext
    {
        return builder
            .AddRedisHealthCheck()
            .AddWolverineHealthCheck()
            .AddDatabaseHealthCheck<TContext>();
    }
}
