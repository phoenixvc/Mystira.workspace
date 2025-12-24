using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Mystira.Shared.Caching;

namespace Mystira.Shared.Health;

/// <summary>
/// Health check for Redis connectivity.
/// Verifies that the distributed cache (Redis) is accessible.
/// </summary>
public class RedisHealthCheck : IHealthCheck
{
    private readonly IDistributedCache _cache;
    private readonly CacheOptions _options;
    private readonly ILogger<RedisHealthCheck> _logger;

    public RedisHealthCheck(
        IDistributedCache cache,
        IOptions<CacheOptions> options,
        ILogger<RedisHealthCheck> logger)
    {
        _cache = cache;
        _options = options.Value;
        _logger = logger;
    }

    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        if (!_options.Enabled)
        {
            return HealthCheckResult.Healthy("Caching is disabled");
        }

        if (string.IsNullOrEmpty(_options.RedisConnectionString))
        {
            return HealthCheckResult.Healthy("Using in-memory cache (Redis not configured)");
        }

        try
        {
            // Try to set and get a test value
            var testKey = $"{_options.InstanceName}healthcheck:{Guid.NewGuid():N}";
            var testValue = DateTime.UtcNow.Ticks.ToString();

            await _cache.SetStringAsync(
                testKey,
                testValue,
                new DistributedCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(30)
                },
                cancellationToken);

            var retrieved = await _cache.GetStringAsync(testKey, cancellationToken);

            // Clean up
            await _cache.RemoveAsync(testKey, cancellationToken);

            if (retrieved == testValue)
            {
                return HealthCheckResult.Healthy("Redis connection successful");
            }

            return HealthCheckResult.Degraded("Redis read/write mismatch");
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Redis health check failed");
            return HealthCheckResult.Unhealthy(
                "Redis connection failed",
                ex,
                new Dictionary<string, object>
                {
                    ["error"] = ex.Message
                });
        }
    }
}
