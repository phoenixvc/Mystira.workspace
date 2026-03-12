using Microsoft.Extensions.Logging;
using Mystira.Core.Ports.Data;
using Mystira.Core.Services;

namespace Mystira.Core.CQRS.MasterData;

/// <summary>
/// Shared helper for master data CRUD operations.
/// Eliminates ~675 lines of duplicated boilerplate across 15 master data command handlers.
/// Each handler delegates to these methods with entity-specific factory/update functions.
/// </summary>
public static class MasterDataCommandHelper
{
    /// <summary>
    /// Generic create operation for master data entities.
    /// </summary>
    public static async Task<T> CreateAsync<T>(
        IMasterDataRepository<T> repository,
        IUnitOfWork unitOfWork,
        IQueryCacheInvalidationService cacheInvalidation,
        ILogger logger,
        string cachePrefix,
        string entityName,
        Func<T> entityFactory,
        CancellationToken ct) where T : class
    {
        var entity = entityFactory();

        await repository.AddAsync(entity);
        await unitOfWork.SaveChangesAsync(ct);

        await cacheInvalidation.InvalidateCacheByPrefixAsync(cachePrefix);

        logger.LogInformation("Successfully created {EntityName}", entityName);
        return entity;
    }

    /// <summary>
    /// Generic update operation for master data entities.
    /// </summary>
    public static async Task<T?> UpdateAsync<T>(
        string id,
        IMasterDataRepository<T> repository,
        IUnitOfWork unitOfWork,
        IQueryCacheInvalidationService cacheInvalidation,
        ILogger logger,
        string cachePrefix,
        string entityName,
        Action<T> applyUpdates,
        CancellationToken ct) where T : class
    {
        var existing = await repository.GetByIdAsync(id);
        if (existing == null)
        {
            logger.LogWarning("{EntityName} with id {Id} not found", entityName, id);
            return null;
        }

        applyUpdates(existing);

        await repository.UpdateAsync(existing);
        await unitOfWork.SaveChangesAsync(ct);

        await cacheInvalidation.InvalidateCacheByPrefixAsync(cachePrefix);

        logger.LogInformation("Successfully updated {EntityName} with id: {Id}", entityName, id);
        return existing;
    }

    /// <summary>
    /// Generic delete operation for master data entities.
    /// </summary>
    public static async Task<bool> DeleteAsync<T>(
        string id,
        IMasterDataRepository<T> repository,
        IUnitOfWork unitOfWork,
        IQueryCacheInvalidationService cacheInvalidation,
        ILogger logger,
        string cachePrefix,
        string entityName,
        CancellationToken ct) where T : class
    {
        var existing = await repository.GetByIdAsync(id);
        if (existing == null)
        {
            logger.LogWarning("{EntityName} with id {Id} not found", entityName, id);
            return false;
        }

        await repository.DeleteAsync(id);
        await unitOfWork.SaveChangesAsync(ct);

        await cacheInvalidation.InvalidateCacheByPrefixAsync(cachePrefix);

        logger.LogInformation("Successfully deleted {EntityName} with id: {Id}", entityName, id);
        return true;
    }
}
