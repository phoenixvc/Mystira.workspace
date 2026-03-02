using Microsoft.Extensions.Logging;
using Mystira.App.Application.CQRS.MasterData;
using Mystira.App.Application.Ports.Data;
using Mystira.App.Application.Services;

namespace Mystira.App.Application.CQRS.FantasyThemes.Commands;

/// <summary>
/// Wolverine handler for deleting a fantasy theme.
/// </summary>
public static class DeleteFantasyThemeCommandHandler
{
    public static async Task<bool> Handle(
        DeleteFantasyThemeCommand command,
        IFantasyThemeRepository repository,
        IUnitOfWork unitOfWork,
        IQueryCacheInvalidationService cacheInvalidation,
        ILogger logger,
        CancellationToken ct)
    {
        return await MasterDataCommandHelper.DeleteAsync(
            command.Id, repository, unitOfWork, cacheInvalidation, logger,
            "MasterData:FantasyThemes", "Fantasy theme", ct);
    }
}
