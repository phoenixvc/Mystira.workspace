using Mystira.Contracts.App.Requests.Scenarios;
using Mystira.Domain.Models;

namespace Mystira.Application.CQRS.Scenarios.Commands;

/// <summary>
/// Command to create a new scenario (write operation)
/// </summary>
public record CreateScenarioCommand(CreateScenarioRequest Request) : ICommand<Scenario>;
