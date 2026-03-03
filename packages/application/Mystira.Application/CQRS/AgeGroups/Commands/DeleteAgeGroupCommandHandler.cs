using Microsoft.Extensions.Logging;
using Mystira.Application.Ports.Data;
using Mystira.Application.Services;

namespace Mystira.Application.CQRS.AgeGroups.Commands;

/// <summary>
/// Wolverine handler for deleting an age group.
/// </summary>
public static class DeleteAgeGroupCommandHandler
{
    /// <summary>
    /// Handles the DeleteAgeGroupCommand.
    /// </summary>
    /// <param name="command">The command to handle.</param>
    /// <param name="repository">The age group repository.</param>
    /// <param name="unitOfWork">The unit of work for transaction management.</param>
    /// <param name="cacheInvalidation">The cache invalidation service.</param>
    /// <param name="logger">The logger instance.</param>
    /// <param name="ct">The cancellation token.</param>
    /// <returns>True if the age group was deleted; otherwise, false.</returns>
    public static async Task<bool> Handle(
        DeleteAgeGroupCommand command,
        IAgeGroupRepository repository,
        IUnitOfWork unitOfWork,
        IQueryCacheInvalidationService cacheInvalidation,
        ILogger logger,
        CancellationToken ct)
    {
        logger.LogInformation("Deleting age group with id: {Id}", command.Id);

        var ageGroup = await repository.GetByIdAsync(command.Id);
        if (ageGroup == null)
        {
            logger.LogWarning("Age group with id {Id} not found", command.Id);
            return false;
        }

        await repository.DeleteAsync(command.Id);
        await unitOfWork.SaveChangesAsync(ct);

        // Invalidate cache
        cacheInvalidation.InvalidateCacheByPrefix("MasterData:AgeGroups");

        logger.LogInformation("Successfully deleted age group with id: {Id}", command.Id);
        return true;
    }
}
