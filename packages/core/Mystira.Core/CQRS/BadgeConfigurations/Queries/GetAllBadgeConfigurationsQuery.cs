using Mystira.Shared.CQRS;
using Mystira.Core.Interfaces;
using Mystira.Core.Ports.Data;
using Mystira.Domain.Models;
using Mystira.Domain.Enums;
using Mystira.Domain.ValueObjects;

namespace Mystira.Core.CQRS.BadgeConfigurations.Queries;

public record GetAllBadgeConfigurationsQuery
    : IQuery<List<BadgeConfiguration>>, ICacheableQuery
{
    public string CacheKey => "BadgeConfigurations:All";
}

public static class GetAllBadgeConfigurationsQueryHandler
{
    public static async Task<List<BadgeConfiguration>> Handle(
        GetAllBadgeConfigurationsQuery request,
        IBadgeConfigurationRepository repository,
        CancellationToken cancellationToken)
    {
        var all = await repository.GetAllAsync(cancellationToken);
        // Stable ordering by Name for deterministic results in tests/clients
        return all.OrderBy(b => b.Name).ToList();
    }
}
