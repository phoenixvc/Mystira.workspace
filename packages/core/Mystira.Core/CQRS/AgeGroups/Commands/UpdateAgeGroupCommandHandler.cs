using Microsoft.Extensions.Logging;
using Mystira.Core.CQRS.MasterData;
using Mystira.Core.Ports.Data;
using Mystira.Core.Services;
using Mystira.Domain.Models;
using Mystira.Domain.Enums;
using Mystira.Domain.ValueObjects;

namespace Mystira.Core.CQRS.AgeGroups.Commands;

/// <summary>
/// Wolverine handler for updating an existing age group.
/// </summary>
public static class UpdateAgeGroupCommandHandler
{
    public static async Task<AgeGroupDefinition?> Handle(
        UpdateAgeGroupCommand command,
        IAgeGroupRepository repository,
        IUnitOfWork unitOfWork,
        IQueryCacheInvalidationService cacheInvalidation,
        ILogger logger,
        CancellationToken ct)
    {
        return await MasterDataCommandHelper.UpdateAsync(
            command.Id, repository, unitOfWork, cacheInvalidation, logger,
            "MasterData:AgeGroups", "Age group",
            existing =>
            {
                existing.Name = command.Name;
                existing.Value = command.Value;
                existing.MinimumAge = command.MinimumAge;
                existing.MaximumAge = command.MaximumAge;
                existing.Description = command.Description;
                existing.UpdatedAt = DateTime.UtcNow;
            }, ct);
    }
}
