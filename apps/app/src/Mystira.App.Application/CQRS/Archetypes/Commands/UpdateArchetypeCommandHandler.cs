using Microsoft.Extensions.Logging;
using Mystira.App.Application.CQRS.MasterData;
using Mystira.Application.Ports.Data;
using Mystira.App.Application.Services;
using Mystira.Domain.Models;
using Mystira.Domain.Enums;
using Mystira.Domain.ValueObjects;

namespace Mystira.App.Application.CQRS.Archetypes.Commands;

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
