using Ardalis.Specification;
using Mystira.Domain.Models;

namespace Mystira.Application.Specifications;

/// <summary>
/// Specification for all active (non-deleted) content bundles.
/// Migrated from ActiveContentBundlesSpecification.
/// </summary>
public sealed class ActiveContentBundlesSpec : BaseEntitySpecification<ContentBundle>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ActiveContentBundlesSpec"/> class.
    /// </summary>
    public ActiveContentBundlesSpec()
    {
        Query.Where(b => true) // In future, add soft delete: !b.IsDeleted
             .OrderBy(b => b.Title);
    }
}

/// <summary>
/// Specification for content bundles by age group.
/// Migrated from ContentBundlesByAgeGroupSpecification.
/// </summary>
public sealed class ContentBundlesByAgeGroupSpec : BaseEntitySpecification<ContentBundle>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ContentBundlesByAgeGroupSpec"/> class.
    /// </summary>
    /// <param name="ageGroup">The age group to filter by.</param>
    public ContentBundlesByAgeGroupSpec(string ageGroup)
    {
        Query.Where(b => b.AgeGroupId == ageGroup)
             .OrderBy(b => b.Title);
    }
}

/// <summary>
/// Specification for free content bundles.
/// Migrated from FreeContentBundlesSpecification.
/// </summary>
public sealed class FreeContentBundlesSpec : BaseEntitySpecification<ContentBundle>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="FreeContentBundlesSpec"/> class.
    /// </summary>
    public FreeContentBundlesSpec()
    {
        Query.Where(b => b.IsFree)
             .OrderBy(b => b.Title);
    }
}

/// <summary>
/// Specification for content bundles by price range.
/// Migrated from ContentBundlesByPriceRangeSpecification.
/// </summary>
public sealed class ContentBundlesByPriceRangeSpec : BaseEntitySpecification<ContentBundle>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ContentBundlesByPriceRangeSpec"/> class.
    /// </summary>
    /// <param name="minPrice">The minimum price to filter by.</param>
    /// <param name="maxPrice">The maximum price to filter by.</param>
    public ContentBundlesByPriceRangeSpec(decimal minPrice, decimal maxPrice)
    {
        Query.Where(b => b.Prices.Any(p => p.Value >= minPrice && p.Value <= maxPrice))
             .OrderBy(b => b.Title);
    }
}

/// <summary>
/// Specification for content bundles containing specific scenarios.
/// Migrated from ContentBundlesByScenarioSpecification.
/// </summary>
public sealed class ContentBundlesByScenarioSpec : BaseEntitySpecification<ContentBundle>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ContentBundlesByScenarioSpec"/> class.
    /// </summary>
    /// <param name="scenarioId">The scenario identifier to filter by.</param>
    public ContentBundlesByScenarioSpec(string scenarioId)
    {
        Query.Where(b => b.ScenarioIds.Contains(scenarioId))
             .OrderBy(b => b.Title);
    }
}

/// <summary>
/// Specification to get a content bundle by ID.
/// </summary>
public sealed class ContentBundleByIdSpec : SingleEntitySpecification<ContentBundle>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ContentBundleByIdSpec"/> class.
    /// </summary>
    /// <param name="id">The content bundle identifier.</param>
    public ContentBundleByIdSpec(string id)
    {
        Query.Where(b => b.Id == id);
    }
}
