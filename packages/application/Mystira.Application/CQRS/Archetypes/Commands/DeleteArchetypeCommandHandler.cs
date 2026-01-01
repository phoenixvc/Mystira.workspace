using Microsoft.Extensions.Logging;
using Mystira.Application.Ports.Data;
using Mystira.Application.Services;

namespace Mystira.Application.CQRS.Archetypes.Commands;

/// <summary>
/// Wolverine handler for deleting an archetype.
/// </summary>
public static class DeleteArchetypeCommandHandler
{
    /// <summary>
    /// Handles the DeleteArchetypeCommand.
    /// </summary>
    /// <param name="command">The command to handle.</param>
    /// <param name="repository">The archetype repository.</param>
    /// <param name="unitOfWork">The unit of work for transaction management.</param>
    /// <param name="cacheInvalidation">The cache invalidation service.</param>
    /// <param name="logger">The logger instance.</param>
    /// <param name="ct">The cancellation token.</param>
    /// <returns>True if the archetype was deleted; otherwise, false.</returns>
    public static async Task<bool> Handle(
        DeleteArchetypeCommand command,
        IArchetypeRepository repository,
        IUnitOfWork unitOfWork,
        IQueryCacheInvalidationService cacheInvalidation,
        ILogger logger,
        CancellationToken ct)
    {
        logger.LogInformation("Deleting archetype with id: {Id}", command.Id);

        var archetype = await repository.GetByIdAsync(command.Id);
        if (archetype == null)
        {
            logger.LogWarning("Archetype with id {Id} not found", command.Id);
            return false;
        }

        await repository.DeleteAsync(command.Id);
        await unitOfWork.SaveChangesAsync(ct);

        // Invalidate cache
        cacheInvalidation.InvalidateCacheByPrefix("MasterData:Archetypes");

        logger.LogInformation("Successfully deleted archetype with id: {Id}", command.Id);
        return true;
    }
}
