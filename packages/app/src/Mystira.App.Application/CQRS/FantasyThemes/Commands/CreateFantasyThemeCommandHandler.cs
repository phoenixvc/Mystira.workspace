using Microsoft.Extensions.Logging;
using Mystira.App.Application.CQRS.MasterData;
using Mystira.App.Application.Ports.Data;
using Mystira.App.Application.Services;
using Mystira.App.Domain.Models;

namespace Mystira.App.Application.CQRS.FantasyThemes.Commands;

/// <summary>
/// Wolverine handler for creating a new fantasy theme.
/// </summary>
public static class CreateFantasyThemeCommandHandler
{
    public static async Task<FantasyThemeDefinition> Handle(
        CreateFantasyThemeCommand command,
        IFantasyThemeRepository repository,
        IUnitOfWork unitOfWork,
        IQueryCacheInvalidationService cacheInvalidation,
        ILogger logger,
        CancellationToken ct)
    {
        Guard.AgainstNullOrEmpty(command.Name, nameof(command.Name));

        return await MasterDataCommandHelper.CreateAsync(
            repository, unitOfWork, cacheInvalidation, logger,
            "MasterData:FantasyThemes", $"fantasy theme '{command.Name}'",
            () => new FantasyThemeDefinition
            {
                Id = Guid.NewGuid().ToString(),
                Name = command.Name,
                Description = command.Description,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            }, ct);
    }
}
