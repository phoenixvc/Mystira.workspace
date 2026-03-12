using Mystira.Domain.Models;
using Mystira.Domain.Enums;
using Mystira.Domain.ValueObjects;

namespace Mystira.Core.UseCases.Scenarios;

/// <summary>
/// Interface for scenario validation use case, enabling testability via mocking.
/// </summary>
public interface IValidateScenarioUseCase
{
    Task ExecuteAsync(Scenario scenario, CancellationToken ct = default);
}
