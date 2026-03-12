using Microsoft.Extensions.Logging;
using Mystira.App.Application.CQRS.MasterData;
using Mystira.Core.Ports.Data;
using Mystira.App.Application.Services;
using Mystira.Domain.Models;
using Mystira.Domain.Enums;
using Mystira.Domain.ValueObjects;

namespace Mystira.App.Application.CQRS.FantasyThemes.Commands;

/// <summary>
/// Wolverine handler for updating an existing fantasy theme.
/// </summary>
public static class UpdateFantasyThemeCommandHandler
{
    public static async Task<FantasyThemeDefinition?> Handle(
        UpdateFantasyThemeCommand command,
        IFantasyThemeRepository repository,
        IUnitOfWork unitOfWork,
        IQueryCacheInvalidationService cacheInvalidation,
        ILogger logger,
        CancellationToken ct)
    {
        return await MasterDataCommandHelper.UpdateAsync(
            command.Id, repository, unitOfWork, cacheInvalidation, logger,
            "MasterData:FantasyThemes", "Fantasy theme",
            existing =>
            {
                existing.Name = command.Name;
                existing.Description = command.Description;
                existing.UpdatedAt = DateTime.UtcNow;
            }, ct);
    }
}
