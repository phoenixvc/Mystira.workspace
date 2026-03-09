using Mystira.App.Domain.Models;

namespace Mystira.App.Application.UseCases.Scenarios;

/// <summary>
/// Interface for scenario validation use case, enabling testability via mocking.
/// </summary>
public interface IValidateScenarioUseCase
{
    Task ExecuteAsync(Scenario scenario, CancellationToken ct = default);
}
