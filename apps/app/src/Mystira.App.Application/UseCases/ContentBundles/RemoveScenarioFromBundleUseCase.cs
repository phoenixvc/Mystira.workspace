using Microsoft.Extensions.Logging;
using Mystira.App.Application.Ports.Data;
using Mystira.App.Domain.Models;
using Mystira.Shared.Exceptions;
using System.Threading;

namespace Mystira.App.Application.UseCases.ContentBundles;

/// <summary>
/// Use case for removing a scenario from a content bundle
/// </summary>
public class RemoveScenarioFromBundleUseCase
{
    private readonly IContentBundleRepository _repository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<RemoveScenarioFromBundleUseCase> _logger;

    public RemoveScenarioFromBundleUseCase(
        IContentBundleRepository repository,
        IUnitOfWork unitOfWork,
        ILogger<RemoveScenarioFromBundleUseCase> logger)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<ContentBundle> ExecuteAsync(string bundleId, string scenarioId, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(bundleId))
        {
            throw new ValidationException("bundleId", "bundleId is required");
        }

        if (string.IsNullOrWhiteSpace(scenarioId))
        {
            throw new ValidationException("scenarioId", "scenarioId is required");
        }

        var bundle = await _repository.GetByIdAsync(bundleId, ct);
        if (bundle == null)
        {
            throw new NotFoundException("ContentBundle", bundleId);
        }

        if (bundle.ScenarioIds != null && bundle.ScenarioIds.Contains(scenarioId))
        {
            bundle.ScenarioIds.Remove(scenarioId);
            await _repository.UpdateAsync(bundle, ct);
            await _unitOfWork.SaveChangesAsync(ct);

            _logger.LogInformation("Removed scenario {ScenarioId} from bundle {BundleId}", scenarioId, bundleId);
        }
        else
        {
            _logger.LogDebug("Scenario {ScenarioId} not found in bundle {BundleId}", scenarioId, bundleId);
        }

        return bundle;
    }
}

