using Microsoft.Extensions.Logging;
using Mystira.Core.CQRS.MasterData;
using Mystira.Core.Ports.Data;
using Mystira.Core.Services;

namespace Mystira.Core.CQRS.FantasyThemes.Commands;

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
