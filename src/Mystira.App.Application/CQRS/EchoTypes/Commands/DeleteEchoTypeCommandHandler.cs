using Microsoft.Extensions.Logging;
using Mystira.App.Application.CQRS.MasterData;
using Mystira.App.Application.Ports.Data;
using Mystira.App.Application.Services;

namespace Mystira.App.Application.CQRS.EchoTypes.Commands;

/// <summary>
/// Wolverine handler for deleting an echo type.
/// </summary>
public static class DeleteEchoTypeCommandHandler
{
    public static async Task<bool> Handle(
        DeleteEchoTypeCommand command,
        IEchoTypeRepository repository,
        IUnitOfWork unitOfWork,
        IQueryCacheInvalidationService cacheInvalidation,
        ILogger logger,
        CancellationToken ct)
    {
        return await MasterDataCommandHelper.DeleteAsync(
            command.Id, repository, unitOfWork, cacheInvalidation, logger,
            "MasterData:EchoTypes", "Echo type", ct);
    }
}
