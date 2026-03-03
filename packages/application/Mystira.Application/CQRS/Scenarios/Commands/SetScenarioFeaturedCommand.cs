namespace Mystira.Application.CQRS.Scenarios.Commands;

/// <summary>
/// Command to set or unset a scenario's featured status (admin operation).
/// </summary>
/// <param name="ScenarioId">The unique identifier of the scenario.</param>
/// <param name="IsFeatured">Whether the scenario should be featured.</param>
public record SetScenarioFeaturedCommand(string ScenarioId, bool IsFeatured) : ICommand;
