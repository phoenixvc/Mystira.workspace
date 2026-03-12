using Microsoft.Extensions.Logging;
using Mystira.App.Application.CQRS.MasterData;
using Mystira.Core.Ports.Data;
using Mystira.App.Application.Services;
using Mystira.Domain.Models;
using Mystira.Domain.Enums;
using Mystira.Domain.ValueObjects;

namespace Mystira.App.Application.CQRS.CompassAxes.Commands;

/// <summary>
/// Wolverine handler for updating an existing compass axis.
/// </summary>
public static class UpdateCompassAxisCommandHandler
{
    public static async Task<CompassAxisDefinition?> Handle(
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
            }, ct);
    }
}
