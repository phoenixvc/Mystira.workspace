using Microsoft.Extensions.Logging;
using Mystira.Application.Ports.Data;
using Mystira.Application.Services;

namespace Mystira.Application.CQRS.EchoTypes.Commands;

/// <summary>
/// Wolverine handler for deleting an echo type.
/// </summary>
public static class DeleteEchoTypeCommandHandler
{
    /// <summary>
    /// Handles the DeleteEchoTypeCommand.
    /// </summary>
    /// <param name="command">The command to handle.</param>
    /// <param name="repository">The echo type repository.</param>
    /// <param name="unitOfWork">The unit of work for transaction management.</param>
    /// <param name="cacheInvalidation">The cache invalidation service.</param>
    /// <param name="logger">The logger instance.</param>
    /// <param name="ct">The cancellation token.</param>
    /// <returns>True if the echo type was deleted; otherwise, false.</returns>
    public static async Task<bool> Handle(
        DeleteEchoTypeCommand command,
        IEchoTypeRepository repository,
        IUnitOfWork unitOfWork,
        IQueryCacheInvalidationService cacheInvalidation,
        ILogger<DeleteEchoTypeCommand> logger,
        CancellationToken ct)
    {
        logger.LogInformation("Deleting echo type with id: {Id}", command.Id);

        var echoType = await repository.GetByIdAsync(command.Id);
        if (echoType == null)
        {
            logger.LogWarning("Echo type with id {Id} not found", command.Id);
            return false;
        }

        await repository.DeleteAsync(command.Id);
        await unitOfWork.SaveChangesAsync(ct);

        // Invalidate cache
        cacheInvalidation.InvalidateCacheByPrefix("MasterData:EchoTypes");

        logger.LogInformation("Successfully deleted echo type with id: {Id}", command.Id);
        return true;
    }
}
