using Mystira.Contracts.App.Requests.Scenarios;
using Mystira.Domain.Models;
using Mystira.Domain.Enums;
using Mystira.Domain.ValueObjects;

namespace Mystira.Core.CQRS.Scenarios.Commands;

/// <summary>
/// Command to create a new scenario (write operation)
/// </summary>
public record CreateScenarioCommand(CreateScenarioRequest Request) : ICommand<Scenario>;
