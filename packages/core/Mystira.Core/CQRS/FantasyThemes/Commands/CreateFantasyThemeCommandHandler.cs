using Microsoft.Extensions.Logging;
using Mystira.Core.CQRS.MasterData;
using Mystira.Core.Ports.Data;
using Mystira.Core.Services;
using Mystira.Domain.Models;
using Mystira.Domain.Enums;
using Mystira.Domain.ValueObjects;

namespace Mystira.Core.CQRS.FantasyThemes.Commands;

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
