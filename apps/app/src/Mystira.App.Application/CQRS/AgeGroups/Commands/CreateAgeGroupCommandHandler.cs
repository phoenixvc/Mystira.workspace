using Microsoft.Extensions.Logging;
using Mystira.App.Application.CQRS.MasterData;
using Mystira.Core.Ports.Data;
using Mystira.App.Application.Services;
using Mystira.Domain.Models;
using Mystira.Domain.Enums;
using Mystira.Domain.ValueObjects;

namespace Mystira.App.Application.CQRS.AgeGroups.Commands;

/// <summary>
/// Wolverine handler for creating a new age group.
/// </summary>
public static class CreateAgeGroupCommandHandler
{
    public static async Task<AgeGroupDefinition> Handle(
        CreateAgeGroupCommand command,
        IAgeGroupRepository repository,
        IUnitOfWork unitOfWork,
        IQueryCacheInvalidationService cacheInvalidation,
        ILogger logger,
        CancellationToken ct)
    {
        Guard.AgainstNullOrEmpty(command.Name, nameof(command.Name));
        Guard.AgainstNullOrEmpty(command.Value, nameof(command.Value));

        return await MasterDataCommandHelper.CreateAsync(
            repository, unitOfWork, cacheInvalidation, logger,
            "MasterData:AgeGroups", $"age group '{command.Name}'",
            () => new AgeGroupDefinition
            {
                Id = Guid.NewGuid().ToString(),
                Name = command.Name,
                Value = command.Value,
                MinimumAge = command.MinimumAge,
                MaximumAge = command.MaximumAge,
                Description = command.Description,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            }, ct);
    }
}
