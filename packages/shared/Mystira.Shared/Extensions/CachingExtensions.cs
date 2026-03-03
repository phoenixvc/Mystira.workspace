using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Mystira.Shared.Caching;

namespace Mystira.Shared.Extensions;

/// <summary>
/// Extension methods for registering caching services.
/// </summary>
public static class CachingExtensions
{
    /// <summary>
    /// Adds Mystira caching services with Redis or in-memory fallback.
    /// Reads configuration from the "Cache" section.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="configuration">The configuration.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddMystiraCaching(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // Register options with validation - fails fast at startup if misconfigured
        services.AddOptions<CacheOptions>()
            .BindConfiguration(CacheOptions.SectionName)
            .ValidateDataAnnotations()
            .ValidateOnStart();

        // Register custom validator for logical constraints
        services.AddSingleton<IValidateOptions<CacheOptions>, CacheOptionsValidator>();

        var options = configuration.GetSection(CacheOptions.SectionName).Get<CacheOptions>()
            ?? new CacheOptions();

        if (!string.IsNullOrEmpty(options.RedisConnectionString))
        {
            // Use Redis distributed cache
            services.AddStackExchangeRedisCache(redisOptions =>
            {
                redisOptions.Configuration = options.RedisConnectionString;
                redisOptions.InstanceName = options.InstanceName;
            });
        }
        else
        {
            // Fall back to in-memory distributed cache for local development
            services.AddDistributedMemoryCache();
        }

        // Use Scoped to match IDistributedCache lifetime and support options reloading
        services.AddScoped<ICacheService, DistributedCacheService>();

        // Add query caching services for Wolverine middleware
        services.AddQueryCaching();

        return services;
    }

    /// <summary>
    /// Adds query caching services for Wolverine middleware.
    /// Includes IQueryCacheInvalidationService and QueryCachingMiddleware.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddQueryCaching(this IServiceCollection services)
    {
        // Add memory cache if not already registered
        services.AddMemoryCache();

        // Register cache invalidation service as singleton (tracks keys across requests)
        services.AddSingleton<IQueryCacheInvalidationService, QueryCacheInvalidationService>();

        // Register middleware as scoped (one per request)
        services.AddScoped<QueryCachingMiddleware>();

        return services;
    }

    /// <summary>
    /// Adds Mystira caching services with explicit Redis connection string.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="redisConnectionString">Redis connection string.</param>
    /// <param name="instanceName">Instance name prefix for keys.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddMystiraCaching(
        this IServiceCollection services,
        string? redisConnectionString,
        string instanceName = "mystira:")
    {
        services.Configure<CacheOptions>(options =>
        {
            options.RedisConnectionString = redisConnectionString;
            options.InstanceName = instanceName;
        });

        // Register custom validator for logical constraints
        services.AddSingleton<IValidateOptions<CacheOptions>, CacheOptionsValidator>();

        if (!string.IsNullOrEmpty(redisConnectionString))
        {
            services.AddStackExchangeRedisCache(options =>
            {
                options.Configuration = redisConnectionString;
                options.InstanceName = instanceName;
            });
        }
        else
        {
            services.AddDistributedMemoryCache();
        }

        // Use Scoped to match IDistributedCache lifetime and support options reloading
        services.AddScoped<ICacheService, DistributedCacheService>();

        return services;
    }
}
