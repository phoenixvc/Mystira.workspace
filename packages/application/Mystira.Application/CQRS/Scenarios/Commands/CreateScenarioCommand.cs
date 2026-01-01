using Mystira.Contracts.App.Requests.Scenarios;
using Mystira.Domain.Models;

namespace Mystira.Application.CQRS.Scenarios.Commands;

/// <summary>
/// Command to create a new scenario (write operation)
/// </summary>
/// <param name="Request">The request containing the scenario data to create.</param>
public record CreateScenarioCommand(CreateScenarioRequest Request) : ICommand<Scenario>;
