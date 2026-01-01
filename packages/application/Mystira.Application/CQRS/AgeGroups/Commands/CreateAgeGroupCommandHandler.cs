using Microsoft.Extensions.Logging;
using Mystira.Application.Ports.Data;
using Mystira.Application.Services;
using Mystira.Domain.Models;

namespace Mystira.Application.CQRS.AgeGroups.Commands;

/// <summary>
/// Wolverine handler for creating a new age group.
/// </summary>
public static class CreateAgeGroupCommandHandler
{
    /// <summary>
    /// Handles the CreateAgeGroupCommand.
    /// </summary>
    /// <param name="command">The command to handle.</param>
    /// <param name="repository">The age group repository.</param>
    /// <param name="unitOfWork">The unit of work for transaction management.</param>
    /// <param name="cacheInvalidation">The cache invalidation service.</param>
    /// <param name="logger">The logger instance.</param>
    /// <param name="ct">The cancellation token.</param>
    /// <returns>The created age group definition.</returns>
    public static async Task<AgeGroupDefinition> Handle(
        CreateAgeGroupCommand command,
        IAgeGroupRepository repository,
        IUnitOfWork unitOfWork,
        IQueryCacheInvalidationService cacheInvalidation,
        ILogger logger,
        CancellationToken ct)
    {
        logger.LogInformation("Creating age group: {Name}", command.Name);

        if (string.IsNullOrWhiteSpace(command.Name))
        {
            throw new ArgumentException("Name is required");
        }

        if (string.IsNullOrWhiteSpace(command.Value))
        {
            throw new ArgumentException("Value is required");
        }

        var ageGroup = new AgeGroupDefinition
        {
            Id = Guid.NewGuid().ToString(),
            Name = command.Name,
            Value = command.Value,
            MinimumAge = command.MinimumAge,
            MaximumAge = command.MaximumAge,
            Description = command.Description,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        await repository.AddAsync(ageGroup);
        await unitOfWork.SaveChangesAsync(ct);

        // Invalidate cache
        cacheInvalidation.InvalidateCacheByPrefix("MasterData:AgeGroups");

        logger.LogInformation("Successfully created age group with id: {Id}", ageGroup.Id);
        return ageGroup;
    }
}
