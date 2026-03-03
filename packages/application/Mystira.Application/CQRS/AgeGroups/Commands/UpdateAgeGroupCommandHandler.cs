using Microsoft.Extensions.Logging;
using Mystira.Application.Ports.Data;
using Mystira.Application.Services;
using Mystira.Domain.Models;

namespace Mystira.Application.CQRS.AgeGroups.Commands;

/// <summary>
/// Wolverine handler for updating an existing age group.
/// </summary>
public static class UpdateAgeGroupCommandHandler
{
    /// <summary>
    /// Handles the UpdateAgeGroupCommand.
    /// </summary>
    /// <param name="command">The command to handle.</param>
    /// <param name="repository">The age group repository.</param>
    /// <param name="unitOfWork">The unit of work for transaction management.</param>
    /// <param name="cacheInvalidation">The cache invalidation service.</param>
    /// <param name="logger">The logger instance.</param>
    /// <param name="ct">The cancellation token.</param>
    /// <returns>The updated age group definition if found; otherwise, null.</returns>
    public static async Task<AgeGroupDefinition?> Handle(
        UpdateAgeGroupCommand command,
        IAgeGroupRepository repository,
        IUnitOfWork unitOfWork,
        IQueryCacheInvalidationService cacheInvalidation,
        ILogger logger,
        CancellationToken ct)
    {
        logger.LogInformation("Updating age group with id: {Id}", command.Id);

        var existingAgeGroup = await repository.GetByIdAsync(command.Id);
        if (existingAgeGroup == null)
        {
            logger.LogWarning("Age group with id {Id} not found", command.Id);
            return null;
        }

        existingAgeGroup.Name = command.Name;
        existingAgeGroup.Value = command.Value;
        existingAgeGroup.MinimumAge = command.MinimumAge;
        existingAgeGroup.MaximumAge = command.MaximumAge;
        existingAgeGroup.Description = command.Description;
        existingAgeGroup.UpdatedAt = DateTime.UtcNow;

        await repository.UpdateAsync(existingAgeGroup);
        await unitOfWork.SaveChangesAsync(ct);

        // Invalidate cache
        cacheInvalidation.InvalidateCacheByPrefix("MasterData:AgeGroups");

        logger.LogInformation("Successfully updated age group with id: {Id}", command.Id);
        return existingAgeGroup;
    }
}
