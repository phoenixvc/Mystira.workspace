using Microsoft.Extensions.Logging;
using Mystira.App.Application.CQRS.MasterData;
using Mystira.Core.Ports.Data;
using Mystira.App.Application.Services;
using Mystira.Domain.Models;
using Mystira.Domain.Enums;
using Mystira.Domain.ValueObjects;

namespace Mystira.App.Application.CQRS.EchoTypes.Commands;

/// <summary>
/// Wolverine handler for creating a new echo type.
/// </summary>
public static class CreateEchoTypeCommandHandler
{
    public static async Task<EchoTypeDefinition> Handle(
        CreateEchoTypeCommand command,
        IEchoTypeRepository repository,
        IUnitOfWork unitOfWork,
        IQueryCacheInvalidationService cacheInvalidation,
        ILogger logger,
        CancellationToken ct)
    {
        Guard.AgainstNullOrEmpty(command.Name, nameof(command.Name));
        Guard.Against(!EchoTypeCategories.IsValid(command.Category),
            $"Category must be one of: {string.Join(", ", EchoTypeCategories.Allowed)}");

        return await MasterDataCommandHelper.CreateAsync(
            repository, unitOfWork, cacheInvalidation, logger,
            "MasterData:EchoTypes", $"echo type '{command.Name}'",
            () => new EchoTypeDefinition
            {
                Id = Guid.NewGuid().ToString(),
                Name = command.Name,
                Description = command.Description,
                Category = command.Category,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            }, ct);
    }
}
