using Microsoft.Extensions.Logging;
using Mystira.Application.Ports.Data;
using Mystira.Application.Services;
using Mystira.Domain.Models;

namespace Mystira.Application.CQRS.Archetypes.Commands;

/// <summary>
/// Wolverine handler for creating a new archetype.
/// </summary>
public static class CreateArchetypeCommandHandler
{
    /// <summary>
    /// Handles the CreateArchetypeCommand.
    /// </summary>
    /// <param name="command">The command to handle.</param>
    /// <param name="repository">The archetype repository.</param>
    /// <param name="unitOfWork">The unit of work for transaction management.</param>
    /// <param name="cacheInvalidation">The cache invalidation service.</param>
    /// <param name="logger">The logger instance.</param>
    /// <param name="ct">The cancellation token.</param>
    /// <returns>The created archetype definition.</returns>
    public static async Task<ArchetypeDefinition> Handle(
        CreateArchetypeCommand command,
        IArchetypeRepository repository,
        IUnitOfWork unitOfWork,
        IQueryCacheInvalidationService cacheInvalidation,
        ILogger logger,
        CancellationToken ct)
    {
        logger.LogInformation("Creating archetype: {Name}", command.Name);

        if (string.IsNullOrWhiteSpace(command.Name))
        {
            throw new ArgumentException("Name is required");
        }

        var archetype = new ArchetypeDefinition
        {
            Id = Guid.NewGuid().ToString(),
            Name = command.Name,
            Description = command.Description,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        await repository.AddAsync(archetype);
        await unitOfWork.SaveChangesAsync(ct);

        // Invalidate cache
        cacheInvalidation.InvalidateCacheByPrefix("MasterData:Archetypes");

        logger.LogInformation("Successfully created archetype with id: {Id}", archetype.Id);
        return archetype;
    }
}
