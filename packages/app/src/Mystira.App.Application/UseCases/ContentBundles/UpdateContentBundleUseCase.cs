using Microsoft.Extensions.Logging;
using Mystira.App.Application.Ports.Data;
using Mystira.App.Domain.Models;
using Mystira.Shared.Exceptions;
using System.Threading;

namespace Mystira.App.Application.UseCases.ContentBundles;

/// <summary>
/// Use case for updating a content bundle
/// </summary>
public class UpdateContentBundleUseCase
{
    private readonly IContentBundleRepository _repository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<UpdateContentBundleUseCase> _logger;

    public UpdateContentBundleUseCase(
        IContentBundleRepository repository,
        IUnitOfWork unitOfWork,
        ILogger<UpdateContentBundleUseCase> logger)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<ContentBundle> ExecuteAsync(
        string bundleId,
        string? title = null,
        string? description = null,
        List<string>? scenarioIds = null,
        string? imageId = null,
        List<BundlePrice>? prices = null,
        bool? isFree = null,
        string? ageGroup = null,
        CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(bundleId))
        {
            throw new ValidationException("bundleId", "bundleId is required");
        }

        var bundle = await _repository.GetByIdAsync(bundleId, ct);
        if (bundle == null)
        {
            throw new NotFoundException("ContentBundle", bundleId);
        }

        // Update properties if provided
        if (title != null)
        {
            bundle.Title = title;
        }

        if (description != null)
        {
            bundle.Description = description;
        }

        if (scenarioIds != null)
        {
            bundle.ScenarioIds = scenarioIds;
        }

        if (imageId != null)
        {
            bundle.ImageId = imageId;
        }

        if (prices != null)
        {
            bundle.Prices = prices;
        }

        if (isFree.HasValue)
        {
            bundle.IsFree = isFree.Value;
        }

        if (ageGroup != null)
        {
            bundle.AgeGroup = ageGroup;
        }

        await _repository.UpdateAsync(bundle, ct);
        await _unitOfWork.SaveChangesAsync(ct);

        _logger.LogInformation("Updated content bundle: {BundleId}", bundleId);
        return bundle;
    }
}

