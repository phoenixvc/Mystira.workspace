using Microsoft.Extensions.Logging;
using Mystira.Application.Ports.Data;
using Mystira.Application.Services;

namespace Mystira.Application.CQRS.CompassAxes.Commands;

/// <summary>
/// Wolverine handler for deleting a compass axis.
/// </summary>
public static class DeleteCompassAxisCommandHandler
{
    /// <summary>
    /// Handles the DeleteCompassAxisCommand.
    /// </summary>
    /// <param name="command">The command to handle.</param>
    /// <param name="repository">The compass axis repository.</param>
    /// <param name="unitOfWork">The unit of work for transaction management.</param>
    /// <param name="cacheInvalidation">The cache invalidation service.</param>
    /// <param name="logger">The logger instance.</param>
    /// <param name="ct">The cancellation token.</param>
    /// <returns>True if the compass axis was deleted; otherwise, false.</returns>
    public static async Task<bool> Handle(
        DeleteCompassAxisCommand command,
        ICompassAxisRepository repository,
        IUnitOfWork unitOfWork,
        IQueryCacheInvalidationService cacheInvalidation,
        ILogger<DeleteCompassAxisCommand> logger,
        CancellationToken ct)
    {
        logger.LogInformation("Deleting compass axis with id: {Id}", command.Id);

        var axis = await repository.GetByIdAsync(command.Id);
        if (axis == null)
        {
            logger.LogWarning("Compass axis with id {Id} not found", command.Id);
            return false;
        }

        await repository.DeleteAsync(command.Id);
        await unitOfWork.SaveChangesAsync(ct);

        // Invalidate cache
        cacheInvalidation.InvalidateCacheByPrefix("MasterData:CompassAxes");

        logger.LogInformation("Successfully deleted compass axis with id: {Id}", command.Id);
        return true;
    }
}
