using Microsoft.Extensions.Logging;
using Mystira.Application.Ports.Data;
using Mystira.Domain.Models;
using Mystira.Domain.Enums;
using Mystira.Domain.ValueObjects;
using Mystira.Shared.Exceptions;
using System.Threading;

namespace Mystira.App.Application.UseCases.ContentBundles;

/// <summary>
/// Use case for adding a scenario to a content bundle
/// </summary>
public class AddScenarioToBundleUseCase
{
    private readonly IContentBundleRepository _repository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<AddScenarioToBundleUseCase> _logger;

    public AddScenarioToBundleUseCase(
        IContentBundleRepository repository,
        IUnitOfWork unitOfWork,
        ILogger<AddScenarioToBundleUseCase> logger)
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

        if (bundle.ScenarioIds == null)
        {
            bundle.ScenarioIds = new List<string>();
        }

        if (!bundle.ScenarioIds.Contains(scenarioId))
        {
            bundle.ScenarioIds.Add(scenarioId);
            await _repository.UpdateAsync(bundle, ct);
            await _unitOfWork.SaveChangesAsync(ct);

            _logger.LogInformation("Added scenario {ScenarioId} to bundle {BundleId}", scenarioId, bundleId);
        }
        else
        {
            _logger.LogDebug("Scenario {ScenarioId} already exists in bundle {BundleId}", scenarioId, bundleId);
        }

        return bundle;
    }
}

