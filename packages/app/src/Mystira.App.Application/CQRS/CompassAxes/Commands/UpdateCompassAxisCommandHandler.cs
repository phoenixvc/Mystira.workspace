using Microsoft.Extensions.Logging;
using Mystira.App.Application.CQRS.MasterData;
using Mystira.App.Application.Ports.Data;
using Mystira.App.Application.Services;
using Mystira.App.Domain.Models;

namespace Mystira.App.Application.CQRS.CompassAxes.Commands;

/// <summary>
/// Wolverine handler for updating an existing compass axis.
/// </summary>
public static class UpdateCompassAxisCommandHandler
{
    public static async Task<CompassAxis?> Handle(
        UpdateCompassAxisCommand command,
        ICompassAxisRepository repository,
        IUnitOfWork unitOfWork,
        IQueryCacheInvalidationService cacheInvalidation,
        ILogger logger,
        CancellationToken ct)
    {
        return await MasterDataCommandHelper.UpdateAsync(
            command.Id, repository, unitOfWork, cacheInvalidation, logger,
            "MasterData:CompassAxes", "Compass axis",
            existing =>
            {
                existing.Name = command.Name;
                existing.Description = command.Description;
                existing.UpdatedAt = DateTime.UtcNow;
            }, ct);
    }
}
