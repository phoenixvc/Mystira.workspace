using Microsoft.Extensions.Logging;
using Mystira.Application.Ports.Data;
using Mystira.Application.Services;
using Mystira.Domain.Models;

namespace Mystira.Application.CQRS.Archetypes.Commands;

/// <summary>
/// Wolverine handler for updating an existing archetype.
/// </summary>
public static class UpdateArchetypeCommandHandler
{
    /// <summary>
    /// Handles the UpdateArchetypeCommand.
    /// </summary>
    /// <param name="command">The command to handle.</param>
    /// <param name="repository">The archetype repository.</param>
    /// <param name="unitOfWork">The unit of work for transaction management.</param>
    /// <param name="cacheInvalidation">The cache invalidation service.</param>
    /// <param name="logger">The logger instance.</param>
    /// <param name="ct">The cancellation token.</param>
    /// <returns>The updated archetype definition if found; otherwise, null.</returns>
    public static async Task<ArchetypeDefinition?> Handle(
        UpdateArchetypeCommand command,
        IArchetypeRepository repository,
        IUnitOfWork unitOfWork,
        IQueryCacheInvalidationService cacheInvalidation,
        ILogger logger,
        CancellationToken ct)
    {
        logger.LogInformation("Updating archetype with id: {Id}", command.Id);

        var existingArchetype = await repository.GetByIdAsync(command.Id);
        if (existingArchetype == null)
        {
            logger.LogWarning("Archetype with id {Id} not found", command.Id);
            return null;
        }

        existingArchetype.Name = command.Name;
        existingArchetype.Description = command.Description;
        existingArchetype.UpdatedAt = DateTime.UtcNow;

        await repository.UpdateAsync(existingArchetype);
        await unitOfWork.SaveChangesAsync(ct);

        // Invalidate cache
        cacheInvalidation.InvalidateCacheByPrefix("MasterData:Archetypes");

        logger.LogInformation("Successfully updated archetype with id: {Id}", command.Id);
        return existingArchetype;
    }
}
