using Microsoft.Extensions.Options;
using Mystira.Admin.Api.Configuration;
using SharedCache = Mystira.Shared.Caching.ICacheService;

namespace Mystira.Admin.Api.Services.Caching;

/// <summary>
/// Adapter that bridges the Admin API's <see cref="ICacheService"/> interface
/// to the shared <see cref="Mystira.Shared.Caching.ICacheService"/> implementation.
///
/// This adapter preserves the Admin API's key-based TTL logic (master data vs. user data
/// vs. content durations) while delegating actual cache operations to the shared
/// <see cref="Mystira.Shared.Caching.DistributedCacheService"/>.
/// </summary>
/// <remarks>
/// Part of Wave 2 monorepo migration: consolidating duplicate ICacheService implementations.
/// The shared ICacheService is the canonical interface going forward.
/// </remarks>
public class SharedCacheServiceAdapter : ICacheService
{
    private readonly SharedCache _sharedCache;
    private readonly RedisCacheOptions _options;
    private readonly ILogger<SharedCacheServiceAdapter> _logger;

    public SharedCacheServiceAdapter(
        SharedCache sharedCache,
        IOptions<RedisCacheOptions> options,
        ILogger<SharedCacheServiceAdapter> logger)
    {
        _sharedCache = sharedCache;
        _options = options.Value;
        _logger = logger;
    }

    /// <inheritdoc />
    public async Task<T?> GetAsync<T>(string key, CancellationToken cancellationToken = default) where T : class
    {
        return await _sharedCache.GetAsync<T>(key, cancellationToken);
    }

    /// <inheritdoc />
    public async Task SetAsync<T>(string key, T value, TimeSpan? expiration = null, CancellationToken cancellationToken = default) where T : class
    {
        var ttl = expiration ?? GetDefaultExpiration(key);
        await _sharedCache.SetAsync(key, value, ttl, cancellationToken);
    }

    /// <inheritdoc />
    public async Task RemoveAsync(string key, CancellationToken cancellationToken = default)
    {
        await _sharedCache.RemoveAsync(key, cancellationToken);
    }

    /// <inheritdoc />
    public async Task RemoveByPatternAsync(string pattern, CancellationToken cancellationToken = default)
    {
        // Delegate to shared implementation first
        await _sharedCache.RemoveByPatternAsync(pattern, cancellationToken);

        // Also invalidate known list keys that match common patterns,
        // preserving the behavior from the original RedisCacheService since
        // IDistributedCache doesn't natively support pattern-based removal.
        if (pattern.Contains("scenario"))
        {
            await _sharedCache.RemoveAsync(CacheKeys.ScenariosList, cancellationToken);
        }
        if (pattern.Contains("character"))
        {
            await _sharedCache.RemoveAsync(CacheKeys.CharacterMapsList, cancellationToken);
        }
        if (pattern.Contains("bundle"))
        {
            await _sharedCache.RemoveAsync(CacheKeys.BundlesList, cancellationToken);
        }
        if (pattern.Contains("badge"))
        {
            await _sharedCache.RemoveAsync(CacheKeys.BadgesList, cancellationToken);
        }
        if (pattern.Contains("master"))
        {
            await _sharedCache.RemoveAsync(CacheKeys.CompassAxes, cancellationToken);
            await _sharedCache.RemoveAsync(CacheKeys.Archetypes, cancellationToken);
            await _sharedCache.RemoveAsync(CacheKeys.EchoTypes, cancellationToken);
            await _sharedCache.RemoveAsync(CacheKeys.FantasyThemes, cancellationToken);
        }
    }

    /// <inheritdoc />
    public async Task<T?> GetOrSetAsync<T>(
        string key,
        Func<CancellationToken, Task<T?>> factory,
        TimeSpan? expiration = null,
        CancellationToken cancellationToken = default) where T : class
    {
        // The shared ICacheService.GetOrCreateAsync expects a factory that returns T (non-nullable)
        // and always caches the result. The Admin API's GetOrSetAsync expects T? and only caches
        // non-null values. We implement this manually to preserve the Admin API's null-aware semantics.
        var cached = await _sharedCache.GetAsync<T>(key, cancellationToken);
        if (cached is not null)
        {
            _logger.LogDebug("Cache hit for key: {Key}", key);
            return cached;
        }

        // Not in cache, call factory
        var value = await factory(cancellationToken);
        if (value is not null)
        {
            var ttl = expiration ?? GetDefaultExpiration(key);
            await _sharedCache.SetAsync(key, value, ttl, cancellationToken);
            _logger.LogDebug("Cache set for key: {Key}, expiration: {Expiration}", key, ttl);
        }

        return value;
    }

    /// <summary>
    /// Determines the default TTL based on the cache key pattern.
    /// Preserves the Admin API's key-based expiration logic:
    /// - Master data keys (":master:") use longer cache durations
    /// - User data keys (":account:", ":profile:") use shorter cache durations
    /// - All other keys use the content cache duration
    /// </summary>
    private TimeSpan GetDefaultExpiration(string key)
    {
        if (key.Contains(":master:"))
        {
            return TimeSpan.FromMinutes(_options.MasterDataCacheMinutes);
        }
        if (key.Contains(":account:") || key.Contains(":profile:"))
        {
            return TimeSpan.FromMinutes(_options.UserCacheMinutes);
        }
        // Default to content cache duration
        return TimeSpan.FromMinutes(_options.ContentCacheMinutes);
    }
}
