using Mystira.Domain.Models;

namespace Mystira.Application.CQRS.Scenarios.Queries;

/// <summary>
/// Query to retrieve paginated scenarios using Specification Pattern
/// Demonstrates CQRS + Specification Pattern with pagination
/// </summary>
/// <param name="PageNumber">The page number to retrieve.</param>
/// <param name="PageSize">The number of items per page.</param>
/// <param name="Search">Optional search term to filter scenarios.</param>
/// <param name="AgeGroup">Optional age group to filter scenarios.</param>
/// <param name="Genre">Optional genre to filter scenarios.</param>
public record GetPaginatedScenariosQuery(
    int PageNumber,
    int PageSize,
    string? Search = null,
    string? AgeGroup = null,
    string? Genre = null) : IQuery<IEnumerable<Scenario>>;
