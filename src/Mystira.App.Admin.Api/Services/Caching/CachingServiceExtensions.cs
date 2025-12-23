using Microsoft.Extensions.Caching.Distributed;
using Mystira.App.Admin.Api.Configuration;

namespace Mystira.App.Admin.Api.Services.Caching;

/// <summary>
/// Extension methods for registering caching services.
/// </summary>
public static class CachingServiceExtensions
{
    /// <summary>
    /// Add caching services to the service collection.
    /// Uses Redis if configured, otherwise falls back to in-memory cache.
    /// </summary>
    public static IServiceCollection AddContentCaching(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var redisConnectionString = configuration.GetConnectionString("Redis");
        var useRedis = !string.IsNullOrEmpty(redisConnectionString);

        if (useRedis)
        {
            // Redis is configured in Program.cs when connection string is present
            // Just register the Redis cache service
            services.AddSingleton<ICacheService, RedisCacheService>();
        }
        else
        {
            // Use in-memory distributed cache for development
            services.AddDistributedMemoryCache();
            services.AddSingleton<ICacheService, InMemoryCacheService>();
        }

        return services;
    }

    /// <summary>
    /// Add cached service decorators.
    /// Call this AFTER registering the base services.
    /// </summary>
    public static IServiceCollection AddCachedServiceDecorators(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var cachingEnabled = configuration.GetValue("DataMigration:EnableContentCaching", defaultValue: true);

        if (!cachingEnabled)
        {
            return services;
        }

        // Decorate IScenarioApiService with caching
        services.Decorate<IScenarioApiService, CachedScenarioApiService>();

        return services;
    }
}

/// <summary>
/// Extension for decorator pattern registration.
/// </summary>
public static class ServiceCollectionDecoratorExtensions
{
    /// <summary>
    /// Decorates an existing service registration with a decorator.
    /// </summary>
    public static IServiceCollection Decorate<TInterface, TDecorator>(this IServiceCollection services)
        where TInterface : class
        where TDecorator : class, TInterface
    {
        // Find the existing registration
        var existingRegistration = services.FirstOrDefault(d => d.ServiceType == typeof(TInterface));

        if (existingRegistration is null)
        {
            throw new InvalidOperationException(
                $"Cannot decorate {typeof(TInterface).Name}: no existing registration found. " +
                $"Register the base service before calling Decorate.");
        }

        // Create a factory that resolves the original service and wraps it
        var existingFactory = existingRegistration.ImplementationFactory;
        var existingType = existingRegistration.ImplementationType;

        services.Remove(existingRegistration);

        // Register the decorator
        services.Add(new ServiceDescriptor(
            typeof(TInterface),
            sp =>
            {
                // Resolve the original implementation
                TInterface inner;
                if (existingFactory is not null)
                {
                    inner = (TInterface)existingFactory(sp);
                }
                else if (existingType is not null)
                {
                    inner = (TInterface)ActivatorUtilities.CreateInstance(sp, existingType);
                }
                else if (existingRegistration.ImplementationInstance is not null)
                {
                    inner = (TInterface)existingRegistration.ImplementationInstance;
                }
                else
                {
                    throw new InvalidOperationException(
                        $"Cannot resolve original implementation of {typeof(TInterface).Name}");
                }

                // Create the decorator, passing the original as the "inner" dependency
                return ActivatorUtilities.CreateInstance<TDecorator>(sp, inner);
            },
            existingRegistration.Lifetime));

        return services;
    }
}
