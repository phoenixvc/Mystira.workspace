using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Mystira.Shared.Data.Polyglot;

/// <summary>
/// Extension methods for configuring polyglot persistence.
/// </summary>
public static class PolyglotExtensions
{
    /// <summary>
    /// Adds polyglot persistence infrastructure with dual database support.
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
        // Configure options
        services.Configure<PolyglotOptions>(configuration.GetSection(PolyglotOptions.SectionName));

        // Register context resolver
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
        });

        services.AddScoped<IDbContextResolver>(sp =>
            new DbContextResolver(sp, null, typeof(TContext)));

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
        services.AddScoped<IPolyglotRepository<TEntity>, PolyglotRepository<TEntity>>();
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
            services.AddScoped(repositoryInterface, repositoryImplementation);
        }

        return services;
    }
}
