using Microsoft.Extensions.Logging;
using Mystira.App.Application.CQRS.MasterData;
using Mystira.App.Application.Ports.Data;
using Mystira.App.Application.Services;
using Mystira.App.Domain.Models;

namespace Mystira.App.Application.CQRS.EchoTypes.Commands;

/// <summary>
/// Wolverine handler for updating an existing echo type.
/// </summary>
public static class UpdateEchoTypeCommandHandler
{
    public static async Task<EchoTypeDefinition?> Handle(
        UpdateEchoTypeCommand command,
        IEchoTypeRepository repository,
        IUnitOfWork unitOfWork,
        IQueryCacheInvalidationService cacheInvalidation,
        ILogger logger,
        CancellationToken ct)
    {
        Guard.AgainstNullOrEmpty(command.Name, nameof(command.Name));
        Guard.Against(!EchoTypeCategories.IsValid(command.Category),
            $"Category must be one of: {string.Join(", ", EchoTypeCategories.Allowed)}");

        return await MasterDataCommandHelper.UpdateAsync(
            command.Id, repository, unitOfWork, cacheInvalidation, logger,
            "MasterData:EchoTypes", "Echo type",
            existing =>
            {
                existing.Name = command.Name;
                existing.Description = command.Description;
                existing.Category = command.Category;
                existing.UpdatedAt = DateTime.UtcNow;
            }, ct);
    }
}
