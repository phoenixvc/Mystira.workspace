using Microsoft.Extensions.Logging;
using Mystira.Application.Ports.Data;
using Mystira.Domain.Models;

namespace Mystira.Application.UseCases.ContentBundles;

/// <summary>
/// Use case for updating a content bundle
/// </summary>
public class UpdateContentBundleUseCase
{
    private readonly IContentBundleRepository _repository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<UpdateContentBundleUseCase> _logger;

    /// <summary>Initializes a new instance of the <see cref="UpdateContentBundleUseCase"/> class.</summary>
    /// <param name="repository">The content bundle repository.</param>
    /// <param name="unitOfWork">The unit of work.</param>
    /// <param name="logger">The logger.</param>
    public UpdateContentBundleUseCase(
        IContentBundleRepository repository,
        IUnitOfWork unitOfWork,
        ILogger<UpdateContentBundleUseCase> logger)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    /// <summary>Updates an existing content bundle.</summary>
    /// <param name="bundleId">The bundle identifier.</param>
    /// <param name="title">The optional new title.</param>
    /// <param name="description">The optional new description.</param>
    /// <param name="scenarioIds">The optional new list of scenario identifiers.</param>
    /// <param name="imageId">The optional new image identifier.</param>
    /// <param name="prices">The optional new list of bundle prices.</param>
    /// <param name="isFree">The optional free flag.</param>
    /// <param name="ageGroup">The optional new age group identifier.</param>
    /// <returns>The updated content bundle.</returns>
    public async Task<ContentBundle> ExecuteAsync(
        string bundleId,
        string? title = null,
        string? description = null,
        List<string>? scenarioIds = null,
        string? imageId = null,
        List<BundlePrice>? prices = null,
        bool? isFree = null,
        string? ageGroup = null)
    {
        if (string.IsNullOrWhiteSpace(bundleId))
        {
            throw new ArgumentException("Bundle ID cannot be null or empty", nameof(bundleId));
        }

        var bundle = await _repository.GetByIdAsync(bundleId);
        if (bundle == null)
        {
            throw new ArgumentException($"Content bundle not found: {bundleId}", nameof(bundleId));
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
            bundle.PriceCents = isFree.Value ? 0 : bundle.PriceCents;
        }

        if (ageGroup != null)
        {
            bundle.AgeGroupId = ageGroup;
        }

        await _repository.UpdateAsync(bundle);
        await _unitOfWork.SaveChangesAsync();

        _logger.LogInformation("Updated content bundle: {BundleId}", bundleId);
        return bundle;
    }
}

