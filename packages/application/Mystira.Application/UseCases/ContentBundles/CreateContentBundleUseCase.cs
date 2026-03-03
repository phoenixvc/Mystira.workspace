using Microsoft.Extensions.Logging;
using Mystira.Application.Ports.Data;
using Mystira.Domain.Models;

namespace Mystira.Application.UseCases.ContentBundles;

/// <summary>
/// Use case for creating a new content bundle
/// </summary>
public class CreateContentBundleUseCase
{
    private readonly IContentBundleRepository _repository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<CreateContentBundleUseCase> _logger;

    /// <summary>Initializes a new instance of the <see cref="CreateContentBundleUseCase"/> class.</summary>
    /// <param name="repository">The content bundle repository.</param>
    /// <param name="unitOfWork">The unit of work.</param>
    /// <param name="logger">The logger.</param>
    public CreateContentBundleUseCase(
        IContentBundleRepository repository,
        IUnitOfWork unitOfWork,
        ILogger<CreateContentBundleUseCase> logger)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    /// <summary>Creates a new content bundle.</summary>
    /// <param name="title">The bundle title.</param>
    /// <param name="description">The bundle description.</param>
    /// <param name="scenarioIds">The list of scenario identifiers.</param>
    /// <param name="imageId">The image identifier.</param>
    /// <param name="prices">The list of bundle prices.</param>
    /// <param name="isFree">Indicates whether the bundle is free.</param>
    /// <param name="ageGroup">The age group identifier.</param>
    /// <returns>The created content bundle.</returns>
    public async Task<ContentBundle> ExecuteAsync(
        string title,
        string description,
        List<string> scenarioIds,
        string imageId,
        List<BundlePrice> prices,
        bool isFree,
        string ageGroup)
    {
        if (string.IsNullOrWhiteSpace(title))
        {
            throw new ArgumentException("Title cannot be null or empty", nameof(title));
        }

        if (scenarioIds == null)
        {
            throw new ArgumentNullException(nameof(scenarioIds));
        }

        var bundle = new ContentBundle
        {
            Id = Guid.NewGuid().ToString("N"),
            Title = title,
            Description = description ?? string.Empty,
            ScenarioIds = scenarioIds,
            ImageId = imageId ?? string.Empty,
            Prices = prices ?? new List<BundlePrice>(),
            PriceCents = isFree ? 0 : (int)Math.Round((prices?.FirstOrDefault()?.Value ?? 0) * 100),
            AgeGroupId = ageGroup ?? string.Empty
        };

        await _repository.AddAsync(bundle);
        await _unitOfWork.SaveChangesAsync();

        _logger.LogInformation("Created content bundle: {BundleId} - {Title}", bundle.Id, bundle.Title);
        return bundle;
    }
}

