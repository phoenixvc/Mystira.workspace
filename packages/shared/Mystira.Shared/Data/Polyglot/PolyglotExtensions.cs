using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Mystira.Shared.Polyglot;

/// <summary>
/// Extension methods for configuring polyglot persistence.
/// </summary>
public static class PolyglotExtensions
{
    /// <summary>
    /// Adds polyglot persistence infrastructure with dual database support.
    /// Supports both SingleStore and DualWrite modes.
    /// </summary>
    /// <typeparam name="TCosmosContext">The Cosmos DB context type.</typeparam>
    /// <typeparam name="TPostgresContext">The PostgreSQL context type.</typeparam>
    /// <param name="services">The service collection.</param>
    /// <param name="configuration">The configuration.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddPolyglotPersistence<TCosmosContext, TPostgresContext>(
        this IServiceCollection services,
        IConfiguration configuration)
        where TCosmosContext : DbContext
        where TPostgresContext : DbContext
    {
        // Configure options from configuration
        services.Configure<PolyglotOptions>(configuration.GetSection(PolyglotOptions.SectionName));

        // Register context resolver with both contexts
        services.AddScoped<IDbContextResolver>(sp =>
            new DbContextResolver(sp, typeof(TCosmosContext), typeof(TPostgresContext)));

        return services;
    }

    /// <summary>
    /// Adds polyglot persistence infrastructure with dual database support and custom options.
    /// </summary>
    /// <typeparam name="TCosmosContext">The Cosmos DB context type.</typeparam>
    /// <typeparam name="TPostgresContext">The PostgreSQL context type.</typeparam>
    /// <param name="services">The service collection.</param>
    /// <param name="configureOptions">Action to configure options.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddPolyglotPersistence<TCosmosContext, TPostgresContext>(
        this IServiceCollection services,
        Action<PolyglotOptions> configureOptions)
        where TCosmosContext : DbContext
        where TPostgresContext : DbContext
    {
        services.Configure(configureOptions);

        services.AddScoped<IDbContextResolver>(sp =>
            new DbContextResolver(sp, typeof(TCosmosContext), typeof(TPostgresContext)));

        return services;
    }

    /// <summary>
    /// Adds polyglot persistence with a single database (Cosmos DB only).
    /// </summary>
    /// <typeparam name="TContext">The Cosmos DB context type.</typeparam>
    /// <param name="services">The service collection.</param>
    /// <param name="configuration">The configuration.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddPolyglotPersistenceCosmos<TContext>(
        this IServiceCollection services,
        IConfiguration configuration)
        where TContext : DbContext
    {
        services.Configure<PolyglotOptions>(options =>
        {
            configuration.GetSection(PolyglotOptions.SectionName).Bind(options);
            options.DefaultTarget = DatabaseTarget.CosmosDb;
            options.Mode = PolyglotMode.SingleStore; // Force single store for single-db setup
        });

        services.AddScoped<IDbContextResolver>(sp =>
            new DbContextResolver(sp, typeof(TContext), null));

        return services;
    }

    /// <summary>
    /// Adds polyglot persistence with a single database (PostgreSQL only).
    /// </summary>
    /// <typeparam name="TContext">The PostgreSQL context type.</typeparam>
    /// <param name="services">The service collection.</param>
    /// <param name="configuration">The configuration.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddPolyglotPersistencePostgres<TContext>(
        this IServiceCollection services,
        IConfiguration configuration)
        where TContext : DbContext
    {
        services.Configure<PolyglotOptions>(options =>
        {
            configuration.GetSection(PolyglotOptions.SectionName).Bind(options);
            options.DefaultTarget = DatabaseTarget.PostgreSql;
            options.Mode = PolyglotMode.SingleStore; // Force single store for single-db setup
        });

        services.AddScoped<IDbContextResolver>(sp =>
            new DbContextResolver(sp, null, typeof(TContext)));

        return services;
    }

    /// <summary>
    /// Adds polyglot persistence in dual-write mode.
    /// Writes to both Cosmos DB and PostgreSQL.
    /// </summary>
    /// <typeparam name="TCosmosContext">The Cosmos DB context type.</typeparam>
    /// <typeparam name="TPostgresContext">The PostgreSQL context type.</typeparam>
    /// <param name="services">The service collection.</param>
    /// <param name="configuration">The configuration.</param>
    /// <param name="primaryTarget">The primary database target (source of truth).</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddPolyglotDualWrite<TCosmosContext, TPostgresContext>(
        this IServiceCollection services,
        IConfiguration configuration,
        DatabaseTarget primaryTarget = DatabaseTarget.CosmosDb)
        where TCosmosContext : DbContext
        where TPostgresContext : DbContext
    {
        services.Configure<PolyglotOptions>(options =>
        {
            configuration.GetSection(PolyglotOptions.SectionName).Bind(options);
            options.Mode = PolyglotMode.DualWrite;
            options.DefaultTarget = primaryTarget;
        });

        services.AddScoped<IDbContextResolver>(sp =>
            new DbContextResolver(sp, typeof(TCosmosContext), typeof(TPostgresContext)));

        return services;
    }

    /// <summary>
    /// Registers a polyglot repository for the specified entity type.
    /// </summary>
    /// <typeparam name="TEntity">The entity type.</typeparam>
    /// <param name="services">The service collection.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddPolyglotRepository<TEntity>(
        this IServiceCollection services)
        where TEntity : class
    {
        services.TryAddScoped<IPolyglotRepository<TEntity>, PolyglotRepository<TEntity>>();
        return services;
    }

    /// <summary>
    /// Registers a polyglot repository with a custom implementation.
    /// </summary>
    /// <typeparam name="TEntity">The entity type.</typeparam>
    /// <typeparam name="TRepository">The repository implementation type.</typeparam>
    /// <param name="services">The service collection.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddPolyglotRepository<TEntity, TRepository>(
        this IServiceCollection services)
        where TEntity : class
        where TRepository : class, IPolyglotRepository<TEntity>
    {
        services.AddScoped<IPolyglotRepository<TEntity>, TRepository>();
        return services;
    }

    /// <summary>
    /// Registers a polyglot backfill service for the specified entity type.
    /// </summary>
    /// <typeparam name="TEntity">The entity type.</typeparam>
    /// <typeparam name="TBackfillService">The backfill service implementation type.</typeparam>
    /// <param name="services">The service collection.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddPolyglotBackfillService<TEntity, TBackfillService>(
        this IServiceCollection services)
        where TEntity : class
        where TBackfillService : class, IPolyglotBackfillService<TEntity>
    {
        services.AddScoped<IPolyglotBackfillService<TEntity>, TBackfillService>();
        return services;
    }

    /// <summary>
    /// Registers polyglot repositories for all entity types in an assembly.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="entityBaseType">Base type that entities inherit from.</param>
    /// <param name="assemblyMarkerType">A type in the assembly containing entities.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddPolyglotRepositoriesFromAssembly(
        this IServiceCollection services,
        Type entityBaseType,
        Type assemblyMarkerType)
    {
        var entityTypes = assemblyMarkerType.Assembly
            .GetTypes()
            .Where(t => t.IsClass && !t.IsAbstract && entityBaseType.IsAssignableFrom(t));

        foreach (var entityType in entityTypes)
        {
            var repositoryInterface = typeof(IPolyglotRepository<>).MakeGenericType(entityType);
            var repositoryImplementation = typeof(PolyglotRepository<>).MakeGenericType(entityType);
            services.TryAddScoped(repositoryInterface, repositoryImplementation);
        }

        return services;
    }

    /// <summary>
    /// Registers polyglot repositories for entities with the specified database target attribute.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="target">The database target to filter entities by.</param>
    /// <param name="assemblyMarkerType">A type in the assembly containing entities.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddPolyglotRepositoriesByTarget(
        this IServiceCollection services,
        DatabaseTarget target,
        Type assemblyMarkerType)
    {
        var entityTypes = assemblyMarkerType.Assembly
            .GetTypes()
            .Where(t =>
            {
                if (!t.IsClass || t.IsAbstract) return false;

                var attr = t.GetCustomAttributes(typeof(DatabaseTargetAttribute), false)
                    .FirstOrDefault() as DatabaseTargetAttribute;

                return attr?.Target == target;
            });

        foreach (var entityType in entityTypes)
        {
            var repositoryInterface = typeof(IPolyglotRepository<>).MakeGenericType(entityType);
            var repositoryImplementation = typeof(PolyglotRepository<>).MakeGenericType(entityType);
            services.TryAddScoped(repositoryInterface, repositoryImplementation);
        }

        return services;
    }
}
