using Microsoft.Extensions.Logging;
using Mystira.Application.Ports.Data;
using Mystira.Application.Services;
using Mystira.Domain.Models;

namespace Mystira.Application.CQRS.EchoTypes.Commands;

/// <summary>
/// Wolverine handler for updating an existing echo type.
/// </summary>
public static class UpdateEchoTypeCommandHandler
{
    /// <summary>
    /// Handles the UpdateEchoTypeCommand.
    /// </summary>
    /// <param name="command">The command to handle.</param>
    /// <param name="repository">The echo type repository.</param>
    /// <param name="unitOfWork">The unit of work for transaction management.</param>
    /// <param name="cacheInvalidation">The cache invalidation service.</param>
    /// <param name="logger">The logger instance.</param>
    /// <param name="ct">The cancellation token.</param>
    /// <returns>The updated echo type definition if found; otherwise, null.</returns>
    public static async Task<EchoTypeDefinition?> Handle(
        UpdateEchoTypeCommand command,
        IEchoTypeRepository repository,
        IUnitOfWork unitOfWork,
        IQueryCacheInvalidationService cacheInvalidation,
        ILogger<UpdateEchoTypeCommand> logger,
        CancellationToken ct)
    {
        logger.LogInformation("Updating echo type with id: {Id}", command.Id);

        var existingEchoType = await repository.GetByIdAsync(command.Id);
        if (existingEchoType == null)
        {
            logger.LogWarning("Echo type with id {Id} not found", command.Id);
            return null;
        }

        existingEchoType.Name = command.Name;
        existingEchoType.Description = command.Description;
        existingEchoType.UpdatedAt = DateTime.UtcNow;

        await repository.UpdateAsync(existingEchoType);
        await unitOfWork.SaveChangesAsync(ct);

        // Invalidate cache
        cacheInvalidation.InvalidateCacheByPrefix("MasterData:EchoTypes");

        logger.LogInformation("Successfully updated echo type with id: {Id}", command.Id);
        return existingEchoType;
    }
}
