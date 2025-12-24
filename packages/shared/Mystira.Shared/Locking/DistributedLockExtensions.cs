using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Mystira.Shared.Locking;

/// <summary>
/// Extension methods for registering distributed lock services.
/// </summary>
public static class DistributedLockExtensions
{
    /// <summary>
    /// Adds Redis-based distributed locking to the service collection.
    /// Requires Redis to be configured via AddStackExchangeRedisCache or AddMystiraCaching.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="configuration">The configuration.</param>
    /// <returns>The service collection for chaining.</returns>
    /// <example>
    /// <code>
    /// builder.Services.AddMystiraCaching(configuration)
    ///     .AddMystiraDistributedLocking(configuration);
    ///
    /// // Use in service
    /// public class OrderService
    /// {
    ///     private readonly IDistributedLockService _lockService;
    ///
    ///     public async Task ProcessOrderAsync(Guid orderId)
    ///     {
    ///         await _lockService.ExecuteWithLockAsync(
    ///             $"order:{orderId}",
    ///             async ct => await DoProcessingAsync(orderId, ct),
    ///             expiry: TimeSpan.FromSeconds(30),
    ///             wait: TimeSpan.FromSeconds(10));
    ///     }
    /// }
    /// </code>
    /// </example>
    public static IServiceCollection AddMystiraDistributedLocking(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.Configure<DistributedLockOptions>(
            configuration.GetSection(DistributedLockOptions.SectionName));

        services.AddSingleton<IDistributedLockService, RedisDistributedLockService>();

        return services;
    }

    /// <summary>
    /// Adds Redis-based distributed locking with custom options.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="configure">Action to configure lock options.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddMystiraDistributedLocking(
        this IServiceCollection services,
        Action<DistributedLockOptions> configure)
    {
        services.Configure(configure);
        services.AddSingleton<IDistributedLockService, RedisDistributedLockService>();

        return services;
    }
}
