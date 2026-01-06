using Ardalis.Specification;
using Mystira.Domain.Models;

namespace Mystira.Application.Specifications;

/// <summary>
/// Specification to get a scenario by ID.
/// </summary>
public sealed class ScenarioByIdSpec : SingleEntitySpecification<Scenario>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ScenarioByIdSpec"/> class.
    /// </summary>
    /// <param name="id">The scenario identifier.</param>
    public ScenarioByIdSpec(string id)
    {
        Query.Where(s => s.Id == id);
    }
}

/// <summary>
/// Specification for scenarios by age group.
/// Migrated from ScenariosByAgeGroupSpecification.
/// </summary>
public sealed class ScenariosByAgeGroupSpec : BaseEntitySpecification<Scenario>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ScenariosByAgeGroupSpec"/> class.
    /// </summary>
    /// <param name="ageGroup">The age group to filter by.</param>
    public ScenariosByAgeGroupSpec(string ageGroup)
    {
        Query.Where(s => s.AgeGroupId == ageGroup)
             .OrderBy(s => s.Title);
    }
}

/// <summary>
/// Specification for scenarios by tag.
/// Migrated from ScenariosByTagSpecification.
/// </summary>
public sealed class ScenariosByTagSpec : BaseEntitySpecification<Scenario>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ScenariosByTagSpec"/> class.
    /// </summary>
    /// <param name="tag">The tag to filter by.</param>
    public ScenariosByTagSpec(string tag)
    {
        Query.Where(s => s.Tags != null && s.Tags.Contains(tag))
             .OrderByDescending(s => s.CreatedAt);
    }
}

/// <summary>
/// Specification for scenarios by difficulty.
/// Migrated from ScenariosByDifficultySpecification.
/// </summary>
public sealed class ScenariosByDifficultySpec : BaseEntitySpecification<Scenario>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ScenariosByDifficultySpec"/> class.
    /// </summary>
    /// <param name="difficulty">The difficulty level to filter by.</param>
    public ScenariosByDifficultySpec(DifficultyLevel difficulty)
    {
        Query.Where(s => s.Difficulty == difficulty)
             .OrderBy(s => s.Title);
    }
}

/// <summary>
/// Specification for scenarios with pagination.
/// Migrated from PaginatedScenariosSpecification.
/// </summary>
public sealed class ScenariosPaginatedSpec : BaseEntitySpecification<Scenario>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ScenariosPaginatedSpec"/> class.
    /// </summary>
    /// <param name="skip">The number of records to skip.</param>
    /// <param name="take">The number of records to take.</param>
    /// <param name="ageGroup">Optional age group to filter by.</param>
    /// <param name="difficulty">Optional difficulty level to filter by.</param>
    /// <param name="tag">Optional tag to filter by.</param>
    public ScenariosPaginatedSpec(
        int skip,
        int take,
        string? ageGroup = null,
        DifficultyLevel? difficulty = null,
        string? tag = null)
    {
        var query = Query.AsTracking();

        if (!string.IsNullOrWhiteSpace(ageGroup))
        {
            query = query.Where(s => s.AgeGroupId == ageGroup);
        }

        if (difficulty.HasValue)
        {
            query = query.Where(s => s.Difficulty == difficulty.Value);
        }

        if (!string.IsNullOrWhiteSpace(tag))
        {
            query = query.Where(s => s.Tags != null && s.Tags.Contains(tag));
        }

        query.OrderByDescending(s => s.CreatedAt)
             .Skip(skip)
             .Take(take);
    }
}

/// <summary>
/// Specification for scenarios by archetype.
/// Migrated from ScenariosByArchetypeSpecification.
/// </summary>
public sealed class ScenariosByArchetypeSpec : BaseEntitySpecification<Scenario>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ScenariosByArchetypeSpec"/> class.
    /// </summary>
    /// <param name="archetypeName">The archetype name to filter by.</param>
    public ScenariosByArchetypeSpec(string archetypeName)
    {
        Query.Where(s => s.Archetypes != null && s.Archetypes.Any(a => a == archetypeName))
             .OrderBy(s => s.Title);
    }
}

/// <summary>
/// Specification for featured scenarios.
/// Uses the IsFeatured boolean property to identify featured scenarios.
/// </summary>
public sealed class FeaturedScenariosSpec : BaseEntitySpecification<Scenario>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="FeaturedScenariosSpec"/> class.
    /// </summary>
    public FeaturedScenariosSpec()
    {
        Query.Where(s => s.IsFeatured && s.IsActive)
             .OrderByDescending(s => s.CreatedAt);
    }
}

/// <summary>
/// Specification to search scenarios by title pattern.
/// </summary>
public sealed class ScenariosByTitlePatternSpec : BaseEntitySpecification<Scenario>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ScenariosByTitlePatternSpec"/> class.
    /// </summary>
    /// <param name="titlePattern">The title pattern to search for.</param>
    public ScenariosByTitlePatternSpec(string titlePattern)
    {
        Query.Where(s => s.Title.ToLower().Contains(titlePattern.ToLower()))
             .OrderBy(s => s.Title);
    }
}

/// <summary>
/// Specification to get all active scenarios.
/// </summary>
public sealed class PublishedScenariosSpec : BaseEntitySpecification<Scenario>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="PublishedScenariosSpec"/> class.
    /// </summary>
    public PublishedScenariosSpec()
    {
        Query.Where(s => s.IsActive)
             .OrderByDescending(s => s.CreatedAt);
    }
}
