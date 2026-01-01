namespace Mystira.Application.CQRS.Scenarios.Commands;

/// <summary>
/// Command to delete a scenario (write operation)
/// </summary>
/// <param name="ScenarioId">The unique identifier of the scenario to delete.</param>
public record DeleteScenarioCommand(string ScenarioId) : ICommand;
