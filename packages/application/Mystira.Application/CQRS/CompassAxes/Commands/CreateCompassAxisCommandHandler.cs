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
