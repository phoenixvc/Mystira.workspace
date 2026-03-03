using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Logging;

namespace Mystira.Infrastructure.Data.Caching;

/// <summary>
/// Extension methods for registering caching services.
/// </summary>
public static class CachingServiceCollectionExtensions
{
    /// <summary>
    /// Add Redis distributed caching with configuration from appsettings.
    /// </summary>
    public static IServiceCollection AddRedisCaching(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // Configure cache options
        services.Configure<CacheOptions>(configuration.GetSection(CacheOptions.SectionName));

        var cacheOptions = new CacheOptions();
        configuration.GetSection(CacheOptions.SectionName).Bind(cacheOptions);

        // Validate Redis configuration if enabled
        if (cacheOptions.Enabled)
        {
            if (string.IsNullOrEmpty(cacheOptions.ConnectionString))
            {
                throw new InvalidOperationException("Redis caching is enabled but ConnectionString is not configured. Please set CacheOptions:ConnectionString in configuration.");
            }

            // Basic connection string validation
            if (!IsValidRedisConnectionString(cacheOptions.ConnectionString))
            {
                throw new InvalidOperationException($"Invalid Redis connection string format: {cacheOptions.ConnectionString}. Expected format: 'server:port' or 'server:port,password=xxx'");
            }

            // Validate instance name
            if (string.IsNullOrEmpty(cacheOptions.InstanceName))
            {
                throw new InvalidOperationException("Redis InstanceName is required when Redis caching is enabled. Please set CacheOptions:InstanceName in configuration.");
            }
        }

        if (!cacheOptions.Enabled || string.IsNullOrEmpty(cacheOptions.ConnectionString))
        {
            // Use in-memory cache as fallback
            services.AddDistributedMemoryCache();
            return services;
        }

        // Configure Redis
        services.AddStackExchangeRedisCache(options =>
        {
            options.Configuration = cacheOptions.ConnectionString;
            options.InstanceName = cacheOptions.InstanceName;
        });

        // Add Redis health check if Redis is enabled
        if (cacheOptions.Enabled && !string.IsNullOrEmpty(cacheOptions.ConnectionString))
        {
            // Note: This requires AspNetCore.HealthChecks.Redis package to be added
            try
            {
                // Check if the health checks package is available by attempting to add the service
                services.AddHealthChecks()
                    .AddRedis(
                        cacheOptions.ConnectionString,
                        name: "redis",
                        failureStatus: HealthStatus.Degraded,
                        tags: new[] { "cache", "redis" });
            }
            catch (Exception ex)
            {
                // Log warning if health checks package is not available
                // This allows the application to continue without health checks
                var loggerFactory = services.BuildServiceProvider().GetService<ILoggerFactory>();
                var logger = loggerFactory?.CreateLogger("CachingServiceCollectionExtensions");
                logger?.LogWarning(ex, "Redis health checks not available. Add AspNetCore.HealthChecks.Redis package to enable.");
            }
        }

        return services;
    }

    /// <summary>
    /// Validates basic Redis connection string format.
    /// </summary>
    /// <param name="connectionString">The Redis connection string to validate.</param>
    /// <returns>True if the connection string appears valid, false otherwise.</returns>
    private static bool IsValidRedisConnectionString(string connectionString)
    {
        if (string.IsNullOrWhiteSpace(connectionString))
            return false;

        // Basic validation for common Redis connection string formats:
        // - server:port
        // - server:port,password=xxx
        // - server:port,ssl=true,sslHost=xxx

        var trimmed = connectionString.Trim();
        if (string.IsNullOrEmpty(trimmed))
            return false;

        var parts = trimmed.Split(',', StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length == 0)
            return false;

        // Check the main server:port part
        var serverPort = parts[0].Trim();
        var serverPortParts = serverPort.Split(':');

        if (serverPortParts.Length != 2)
            return false;

        // Validate server name (non-empty)
        if (string.IsNullOrWhiteSpace(serverPortParts[0]))
            return false;

        // Validate port (numeric and in valid range)
        if (!int.TryParse(serverPortParts[1], out var port) || port <= 0 || port > 65535)
            return false;

        return true;
    }

    /// <summary>
    /// Decorate a repository with caching.
    /// Uses Scrutor-style decoration pattern.
    /// </summary>
    /// <example>
    /// services.AddCachedRepository{Account}();
    /// </example>
    public static IServiceCollection AddCachedRepository<TEntity>(
        this IServiceCollection services)
        where TEntity : class
    {
        // Register the cached decorator
        // This requires the base ISpecRepository to be registered first
        services.Decorate<Application.Ports.Data.ISpecRepository<TEntity>, CachedRepository<TEntity>>();
        return services;
    }
}

/// <summary>
/// Simple decorator extension (if Scrutor is not available).
/// </summary>
public static class DecoratorExtensions
{
    /// <summary>
    /// Decorate a service with a decorator implementation.
    /// Removes the original registration and replaces it with the decorated version.
    /// </summary>
    public static IServiceCollection Decorate<TInterface, TDecorator>(this IServiceCollection services)
        where TInterface : class
        where TDecorator : class, TInterface
    {
        var wrappedDescriptor = services.FirstOrDefault(s => s.ServiceType == typeof(TInterface));

        if (wrappedDescriptor == null)
        {
            throw new InvalidOperationException($"Service of type {typeof(TInterface).Name} not registered");
        }

        // Remove the original descriptor to avoid DI resolution ambiguity
        services.Remove(wrappedDescriptor);

        var objectFactory = ActivatorUtilities.CreateFactory(
            typeof(TDecorator),
            new[] { typeof(TInterface) });

        // Add the decorated service with the same lifetime as the original
        services.Add(ServiceDescriptor.Describe(
            typeof(TInterface),
            sp => (TInterface)objectFactory(sp, new[] { CreateInstance(sp, wrappedDescriptor) }),
            wrappedDescriptor.Lifetime));

        return services;
    }

    private static object CreateInstance(IServiceProvider services, ServiceDescriptor descriptor)
    {
        if (descriptor.ImplementationInstance != null)
        {
            return descriptor.ImplementationInstance;
        }

        if (descriptor.ImplementationFactory != null)
        {
            return descriptor.ImplementationFactory(services);
        }

        return ActivatorUtilities.GetServiceOrCreateInstance(services, descriptor.ImplementationType!);
    }
}
