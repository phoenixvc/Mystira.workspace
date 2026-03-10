using Ardalis.Specification;
using Mystira.Domain.Models;
using Mystira.Domain.Enums;
using Mystira.Domain.ValueObjects;

namespace Mystira.App.Application.Specifications;

/// <summary>
/// Specification to get all badge configurations.
/// </summary>
public sealed class AllBadgeConfigurationsSpec : BaseEntitySpecification<BadgeConfiguration>
{
    public AllBadgeConfigurationsSpec()
    {
        Query.OrderBy(b => b.Axis);
    }
}
