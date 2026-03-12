using Microsoft.Extensions.Logging;
using Mystira.Core.CQRS.MasterData;
using Mystira.Core.Ports.Data;
using Mystira.Core.Services;

namespace Mystira.Core.CQRS.CompassAxes.Commands;

/// <summary>
/// Wolverine handler for deleting a compass axis.
/// </summary>
public static class DeleteCompassAxisCommandHandler
{
    public static async Task<bool> Handle(
        DeleteCompassAxisCommand command,
        ICompassAxisRepository repository,
        IUnitOfWork unitOfWork,
        IQueryCacheInvalidationService cacheInvalidation,
        ILogger logger,
        CancellationToken ct)
    {
        return await MasterDataCommandHelper.DeleteAsync(
            command.Id, repository, unitOfWork, cacheInvalidation, logger,
            "MasterData:CompassAxes", "Compass axis", ct);
    }
}
