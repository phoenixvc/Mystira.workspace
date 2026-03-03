using Microsoft.Extensions.Logging;
using Mystira.Application.Ports.Data;
using Mystira.Application.Services;
using Mystira.Domain.Models;

namespace Mystira.Application.CQRS.CompassAxes.Commands;

/// <summary>
/// Wolverine handler for updating an existing compass axis.
/// </summary>
public static class UpdateCompassAxisCommandHandler
{
    /// <summary>
    /// Handles the UpdateCompassAxisCommand.
    /// </summary>
    /// <param name="command">The command to handle.</param>
    /// <param name="repository">The compass axis repository.</param>
    /// <param name="unitOfWork">The unit of work for transaction management.</param>
    /// <param name="cacheInvalidation">The cache invalidation service.</param>
    /// <param name="logger">The logger instance.</param>
    /// <param name="ct">The cancellation token.</param>
    /// <returns>The updated compass axis definition if found; otherwise, null.</returns>
    public static async Task<CompassAxisDefinition?> Handle(
        UpdateCompassAxisCommand command,
        ICompassAxisRepository repository,
        IUnitOfWork unitOfWork,
        IQueryCacheInvalidationService cacheInvalidation,
        ILogger<UpdateCompassAxisCommand> logger,
        CancellationToken ct)
    {
        logger.LogInformation("Updating compass axis with id: {Id}", command.Id);

        var existingAxis = await repository.GetByIdAsync(command.Id);
        if (existingAxis == null)
        {
            logger.LogWarning("Compass axis with id {Id} not found", command.Id);
            return null;
        }

        existingAxis.Name = command.Name;
        existingAxis.Description = command.Description ?? string.Empty;
        existingAxis.UpdatedAt = DateTime.UtcNow;

        await repository.UpdateAsync(existingAxis);
        await unitOfWork.SaveChangesAsync(ct);

        // Invalidate cache
        cacheInvalidation.InvalidateCacheByPrefix("MasterData:CompassAxes");

        logger.LogInformation("Successfully updated compass axis with id: {Id}", command.Id);
        return existingAxis;
    }
}
