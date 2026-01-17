using Ardalis.Specification;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Mystira.Shared.Data.Repositories;

namespace Mystira.Shared.Extensions;

/// <summary>
/// Extension methods for registering data services.
/// </summary>
public static class DataExtensions
{
    /// <summary>
    /// Adds a generic repository base for the specified entity type that implements IRepositoryBase.
    /// </summary>
    /// <remarks>
    /// This registers the repository as <see cref="IRepositoryBase{TEntity}"/> from Ardalis.Specification.
    /// For full IRepository functionality, use <c>Mystira.Infrastructure.Data</c> package which provides
    /// repositories implementing <c>Mystira.Application.Ports.Data.IRepository&lt;TEntity&gt;</c>.
    /// </remarks>
    /// <typeparam name="TEntity">The entity type.</typeparam>
    /// <typeparam name="TContext">The DbContext type.</typeparam>
    /// <param name="services">The service collection.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddRepositoryBase<TEntity, TContext>(
        this IServiceCollection services)
        where TEntity : class
        where TContext : DbContext
    {
        services.AddScoped<IRepositoryBase<TEntity>>(provider =>
        {
            var context = provider.GetRequiredService<TContext>();
            return new RepositoryBase<TEntity>(context);
        });

        return services;
    }

    /// <summary>
    /// Adds a custom repository implementation.
    /// </summary>
    /// <typeparam name="TInterface">The repository interface.</typeparam>
    /// <typeparam name="TImplementation">The repository implementation.</typeparam>
    /// <param name="services">The service collection.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddCustomRepository<TInterface, TImplementation>(
        this IServiceCollection services)
        where TInterface : class
        where TImplementation : class, TInterface
    {
        services.AddScoped<TInterface, TImplementation>();
        return services;
    }

    /// <summary>
    /// Adds a unit of work implementation.
    /// </summary>
    /// <typeparam name="TUnitOfWork">The unit of work implementation.</typeparam>
    /// <param name="services">The service collection.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddUnitOfWork<TUnitOfWork>(
        this IServiceCollection services)
        where TUnitOfWork : class, IUnitOfWork
    {
        services.AddScoped<IUnitOfWork, TUnitOfWork>();
        return services;
    }
}
