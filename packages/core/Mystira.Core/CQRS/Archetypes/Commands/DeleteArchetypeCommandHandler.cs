using Microsoft.Extensions.Logging;
using Mystira.Core.CQRS.MasterData;
using Mystira.Core.Ports.Data;
using Mystira.Core.Services;
using Mystira.Domain.Models;
using Mystira.Domain.Enums;
using Mystira.Domain.ValueObjects;

namespace Mystira.Core.CQRS.Archetypes.Commands;

/// <summary>
/// Wolverine handler for deleting an archetype.
/// </summary>
public static class DeleteArchetypeCommandHandler
{
    public static async Task<bool> Handle(
        DeleteArchetypeCommand command,
        IArchetypeRepository repository,
        IUnitOfWork unitOfWork,
        IQueryCacheInvalidationService cacheInvalidation,
        ILogger logger,
        CancellationToken ct)
    {
        return await MasterDataCommandHelper.DeleteAsync(
            command.Id, repository, unitOfWork, cacheInvalidation, logger,
            "MasterData:Archetypes", "Archetype", ct);
    }
}
