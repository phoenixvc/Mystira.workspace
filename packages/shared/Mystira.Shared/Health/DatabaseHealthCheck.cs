using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Logging;

namespace Mystira.Shared.Health;

/// <summary>
/// Health check for database connectivity using Entity Framework Core.
/// </summary>
/// <typeparam name="TContext">The DbContext type to check.</typeparam>
public class DatabaseHealthCheck<TContext> : IHealthCheck where TContext : DbContext
{
    private readonly TContext _context;
    private readonly ILogger<DatabaseHealthCheck<TContext>> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="DatabaseHealthCheck{TContext}"/> class.
    /// </summary>
    /// <param name="context">The database context to check.</param>
    /// <param name="logger">Logger instance.</param>
    public DatabaseHealthCheck(
        TContext context,
        ILogger<DatabaseHealthCheck<TContext>> logger)
    {
        _context = context;
        _logger = logger;
    }

    /// <inheritdoc />
    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        try
        {
            // Check if the database can be connected to
            var canConnect = await _context.Database.CanConnectAsync(cancellationToken);

            if (!canConnect)
            {
                return HealthCheckResult.Unhealthy("Cannot connect to database");
            }

            // Optionally check for pending migrations
            var pendingMigrations = await _context.Database.GetPendingMigrationsAsync(cancellationToken);
            var pendingCount = pendingMigrations.Count();

            if (pendingCount > 0)
            {
                return HealthCheckResult.Degraded(
                    $"Database connected but has {pendingCount} pending migrations",
                    data: new Dictionary<string, object>
                    {
                        ["pendingMigrations"] = pendingMigrations.ToList()
                    });
            }

            return HealthCheckResult.Healthy("Database connection successful");
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Database health check failed for {Context}", typeof(TContext).Name);
            return HealthCheckResult.Unhealthy(
                "Database connection failed",
                ex,
                new Dictionary<string, object>
                {
                    ["error"] = ex.Message,
                    ["context"] = typeof(TContext).Name
                });
        }
    }
}
