using Microsoft.Extensions.Logging;
using Mystira.Application.Ports.Data;

namespace Mystira.Application.UseCases.Scenarios;

/// <summary>
/// Use case for deleting a scenario
/// </summary>
public class DeleteScenarioUseCase
{
    private readonly IScenarioRepository _repository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<DeleteScenarioUseCase> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="DeleteScenarioUseCase"/> class.
    /// </summary>
    /// <param name="repository">The scenario repository.</param>
    /// <param name="unitOfWork">The unit of work for transaction management.</param>
    /// <param name="logger">The logger instance.</param>
    public DeleteScenarioUseCase(
        IScenarioRepository repository,
        IUnitOfWork unitOfWork,
        ILogger<DeleteScenarioUseCase> logger)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    /// <summary>
    /// Deletes a scenario by its identifier.
    /// </summary>
    /// <param name="id">The scenario identifier.</param>
    /// <returns>True if the scenario was deleted; false if not found.</returns>
    public async Task<bool> ExecuteAsync(string id)
    {
        var scenario = await _repository.GetByIdAsync(id);
        if (scenario == null)
        {
            return false;
        }

        await _repository.DeleteAsync(id);
        await _unitOfWork.SaveChangesAsync();

        _logger.LogInformation("Deleted scenario: {ScenarioId} - {Title}", scenario.Id, scenario.Title);
        return true;
    }
}

