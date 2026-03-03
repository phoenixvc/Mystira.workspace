using Mystira.Shared.CQRS;
using Mystira.App.Application.Interfaces;
using Mystira.App.Application.Ports.Data;
using Mystira.App.Domain.Models;

namespace Mystira.App.Application.CQRS.BadgeConfigurations.Queries;

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
