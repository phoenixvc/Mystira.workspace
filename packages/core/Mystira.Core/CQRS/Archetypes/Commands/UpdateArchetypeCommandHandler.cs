using Microsoft.Extensions.Logging;
using Mystira.Core.CQRS.MasterData;
using Mystira.Core.Ports.Data;
using Mystira.Core.Services;
using Mystira.Domain.Models;
using Mystira.Domain.Enums;
using Mystira.Domain.ValueObjects;

namespace Mystira.Core.CQRS.Archetypes.Commands;

/// <summary>
/// Wolverine handler for updating an existing archetype.
/// </summary>
public static class UpdateArchetypeCommandHandler
{
    public static async Task<ArchetypeDefinition?> Handle(
        UpdateArchetypeCommand command,
        IArchetypeRepository repository,
        IUnitOfWork unitOfWork,
        IQueryCacheInvalidationService cacheInvalidation,
        ILogger logger,
        CancellationToken ct)
    {
        return await MasterDataCommandHelper.UpdateAsync(
            command.Id, repository, unitOfWork, cacheInvalidation, logger,
            "MasterData:Archetypes", "Archetype",
            existing =>
            {
                existing.Name = command.Name;
                existing.Description = command.Description;
                existing.UpdatedAt = DateTime.UtcNow;
            }, ct);
    }
}
