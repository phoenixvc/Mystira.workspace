using Ardalis.Specification;
using Mystira.Domain.Models;

namespace Mystira.Application.Specifications;

/// <summary>
/// Specification to get all badge configurations.
/// </summary>
public sealed class AllBadgeConfigurationsSpec : BaseEntitySpecification<BadgeConfiguration>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="AllBadgeConfigurationsSpec"/> class.
    /// </summary>
    public AllBadgeConfigurationsSpec()
    {
        Query.OrderBy(b => b.Axis);
    }
}
