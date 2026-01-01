using Mystira.Domain.Models;

namespace Mystira.Application.CQRS.Scenarios.Queries;

/// <summary>
/// Query to retrieve scenarios by age group using Specification Pattern
/// Demonstrates CQRS + Specification Pattern integration
/// </summary>
/// <param name="AgeGroup">The age group to filter scenarios by.</param>
public record GetScenariosByAgeGroupQuery(string AgeGroup) : IQuery<IEnumerable<Scenario>>;
