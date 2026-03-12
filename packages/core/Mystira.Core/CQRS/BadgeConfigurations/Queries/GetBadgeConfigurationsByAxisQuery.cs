using Mystira.Shared.CQRS;
using Mystira.Core.Interfaces;
using Mystira.Core.Ports.Data;
using Mystira.Domain.Models;
using Mystira.Domain.Enums;
using Mystira.Domain.ValueObjects;

namespace Mystira.Core.CQRS.BadgeConfigurations.Queries;

public record GetBadgeConfigurationsByAxisQuery(string Axis)
    : IQuery<List<BadgeConfiguration>>, ICacheableQuery
{
    public string CacheKey => $"BadgeConfigurations:Axis:{Axis}";
}

public static class GetBadgeConfigurationsByAxisQueryHandler
{
    public static async Task<List<BadgeConfiguration>> Handle(
        GetBadgeConfigurationsByAxisQuery request,
        IBadgeConfigurationRepository repository,
        CancellationToken cancellationToken)
    {
        var list = await repository.GetByAxisAsync(request.Axis, cancellationToken);
        return list.OrderBy(b => b.Name).ToList();
    }
}
