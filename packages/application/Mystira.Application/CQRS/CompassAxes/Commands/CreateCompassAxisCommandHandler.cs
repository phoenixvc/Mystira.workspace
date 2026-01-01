using Microsoft.Extensions.Logging;
using Mystira.Application.Ports.Data;
using Mystira.Application.Services;
using Mystira.Domain.Models;

namespace Mystira.Application.CQRS.CompassAxes.Commands;

/// <summary>
/// Wolverine handler for creating a new compass axis.
/// </summary>
public static class CreateCompassAxisCommandHandler
{
    /// <summary>
    /// Handles the CreateCompassAxisCommand.
    /// </summary>
    /// <param name="command">The command to handle.</param>
    /// <param name="repository">The compass axis repository.</param>
    /// <param name="unitOfWork">The unit of work for transaction management.</param>
    /// <param name="cacheInvalidation">The cache invalidation service.</param>
    /// <param name="logger">The logger instance.</param>
    /// <param name="ct">The cancellation token.</param>
    /// <returns>The created compass axis definition.</returns>
    public static async Task<CompassAxisDefinition> Handle(
        CreateCompassAxisCommand command,
        ICompassAxisRepository repository,
        IUnitOfWork unitOfWork,
        IQueryCacheInvalidationService cacheInvalidation,
        ILogger<CreateCompassAxisCommand> logger,
        CancellationToken ct)
    {
        logger.LogInformation("Creating compass axis: {Name}", command.Name);

        if (string.IsNullOrWhiteSpace(command.Name))
        {
            throw new ArgumentException("Name is required");
        }

        var axis = new CompassAxisDefinition
        {
            Id = Guid.NewGuid().ToString(),
            Name = command.Name,
            Description = command.Description ?? string.Empty,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        await repository.AddAsync(axis);
        await unitOfWork.SaveChangesAsync(ct);

        // Invalidate cache
        cacheInvalidation.InvalidateCacheByPrefix("MasterData:CompassAxes");

        logger.LogInformation("Successfully created compass axis with id: {Id}", axis.Id);
        return axis;
    }
}
