using Microsoft.Extensions.Logging;
using Mystira.Core.CQRS.MasterData;
using Mystira.Core.Ports.Data;
using Mystira.Core.Services;
using Mystira.Domain.Models;
using Mystira.Domain.Enums;
using Mystira.Domain.ValueObjects;

namespace Mystira.Core.CQRS.Archetypes.Commands;

/// <summary>
/// Wolverine handler for creating a new archetype.
/// </summary>
public static class CreateArchetypeCommandHandler
{
    public static async Task<ArchetypeDefinition> Handle(
        CreateArchetypeCommand command,
        IArchetypeRepository repository,
        IUnitOfWork unitOfWork,
        IQueryCacheInvalidationService cacheInvalidation,
        ILogger logger,
        CancellationToken ct)
    {
        Guard.AgainstNullOrEmpty(command.Name, nameof(command.Name));

        return await MasterDataCommandHelper.CreateAsync(
            repository, unitOfWork, cacheInvalidation, logger,
            "MasterData:Archetypes", $"archetype '{command.Name}'",
            () => new ArchetypeDefinition
            {
                Id = Guid.NewGuid().ToString(),
                Name = command.Name,
                Description = command.Description,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            }, ct);
    }
}
