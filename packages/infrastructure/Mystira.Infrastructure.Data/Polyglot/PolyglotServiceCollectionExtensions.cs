using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Mystira.Application.Ports.Data;

namespace Mystira.Infrastructure.Data.Polyglot;

/// <summary>
/// Extension methods for registering polyglot persistence services.
/// </summary>
public static class PolyglotServiceCollectionExtensions
{
    /// <summary>
    /// Add polyglot persistence services with the specified DbContext as primary.
    /// </summary>
    /// <typeparam name="TContext">The primary DbContext type</typeparam>
    public static IServiceCollection AddPolyglotPersistence<TContext>(
        this IServiceCollection services,
        IConfiguration configuration)
        where TContext : DbContext
    {
        // Configure migration options
        services.Configure<MigrationOptions>(
            configuration.GetSection(MigrationOptions.SectionName));

        // Register the EF specification repository for standard use
        services.AddScoped(typeof(ISpecRepository<>), typeof(EfSpecificationRepository<>));
        services.AddScoped(typeof(IReadRepository<>), typeof(EfSpecificationRepository<>));

        // Register the polyglot repository for entities that need migration support
        services.AddScoped(typeof(IPolyglotRepository<>), typeof(PolyglotRepository<>));

        return services;
    }

    /// <summary>
    /// Add polyglot persistence with dual-write support between two DbContexts.
    /// Both contexts must be registered separately with their connection strings before calling this method.
    ///
    /// Note: After calling this method, use <see cref="AddPolyglotEntityWithDualWrite{TEntity, TPrimaryContext, TSecondaryContext}"/>
    /// to register specific entities that need dual-write support.
    /// </summary>
    /// <typeparam name="TPrimaryContext">The primary DbContext (e.g., Cosmos DB)</typeparam>
    /// <typeparam name="TSecondaryContext">The secondary DbContext (e.g., PostgreSQL)</typeparam>
    /// <param name="services">The service collection</param>
    /// <param name="configuration">The configuration instance</param>
    /// <returns>The service collection for chaining</returns>
    public static IServiceCollection AddPolyglotPersistenceWithDualWrite<TPrimaryContext, TSecondaryContext>(
        this IServiceCollection services,
        IConfiguration configuration)
        where TPrimaryContext : DbContext
        where TSecondaryContext : DbContext
    {
        // Configure migration options with dual-write mode
        services.Configure<MigrationOptions>(
            configuration.GetSection(MigrationOptions.SectionName));

        services.Configure<PolyglotOptions>(opts => opts.Mode = PolyglotMode.DualWrite);

        // Register standard repositories using primary context
        services.AddScoped(typeof(ISpecRepository<>), typeof(EfSpecificationRepository<>));
        services.AddScoped(typeof(IReadRepository<>), typeof(EfSpecificationRepository<>));

        // Register DbContext as the primary context type for default resolution
        services.AddScoped<DbContext>(sp => sp.GetRequiredService<TPrimaryContext>());

        return services;
    }

    /// <summary>
    /// Add a specific entity type to use polyglot repository with dual-write across two contexts.
    /// </summary>
    /// <typeparam name="TEntity">The entity type</typeparam>
    /// <typeparam name="TPrimaryContext">The primary DbContext type</typeparam>
    /// <typeparam name="TSecondaryContext">The secondary DbContext type</typeparam>
    /// <param name="services">The service collection</param>
    /// <returns>The service collection for chaining</returns>
    public static IServiceCollection AddPolyglotEntityWithDualWrite<TEntity, TPrimaryContext, TSecondaryContext>(
        this IServiceCollection services)
        where TEntity : class
        where TPrimaryContext : DbContext
        where TSecondaryContext : DbContext
    {
        services.AddScoped<IPolyglotRepository<TEntity>>(sp =>
        {
            var primaryContext = sp.GetRequiredService<TPrimaryContext>();
            var secondaryContext = sp.GetRequiredService<TSecondaryContext>();
            var options = sp.GetRequiredService<Microsoft.Extensions.Options.IOptions<PolyglotOptions>>();
            var logger = sp.GetRequiredService<Microsoft.Extensions.Logging.ILogger<PolyglotRepository<TEntity>>>();

            return new PolyglotRepository<TEntity>(primaryContext, options, logger, secondaryContext);
        });

        return services;
    }

    /// <summary>
    /// Add a specific entity type to use polyglot repository with dual-write.
    /// </summary>
    public static IServiceCollection AddPolyglotEntity<TEntity, TContext>(
        this IServiceCollection services)
        where TEntity : class
        where TContext : DbContext
    {
        services.AddScoped<IPolyglotRepository<TEntity>>(sp =>
        {
            var context = sp.GetRequiredService<TContext>();
            var options = sp.GetRequiredService<Microsoft.Extensions.Options.IOptions<MigrationOptions>>();
            var logger = sp.GetRequiredService<Microsoft.Extensions.Logging.ILogger<PolyglotRepository<TEntity>>>();

            return new PolyglotRepository<TEntity>(context, options, logger);
        });

        return services;
    }
}
