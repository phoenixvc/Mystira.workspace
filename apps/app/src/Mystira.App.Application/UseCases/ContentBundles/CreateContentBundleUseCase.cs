using Microsoft.Extensions.Logging;
using Mystira.Application.Ports.Data;
using Mystira.Domain.Models;
using Mystira.Domain.Enums;
using Mystira.Domain.ValueObjects;
using Mystira.Shared.Exceptions;
using System.Threading;

namespace Mystira.App.Application.UseCases.ContentBundles;

/// <summary>
/// Use case for creating a new content bundle
/// </summary>
public class CreateContentBundleUseCase
{
    private readonly IContentBundleRepository _repository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<CreateContentBundleUseCase> _logger;

    public CreateContentBundleUseCase(
        IContentBundleRepository repository,
        IUnitOfWork unitOfWork,
        ILogger<CreateContentBundleUseCase> logger)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<ContentBundle> ExecuteAsync(
        string title,
        string description,
        List<string> scenarioIds,
        string imageId,
        List<BundlePrice> prices,
        bool isFree,
        string ageGroup,
        CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(title))
        {
            throw new ValidationException("title", "title is required");
        }

        if (scenarioIds == null)
        {
            throw new ValidationException("scenarioIds", "scenarioIds is required");
        }

        var bundle = new ContentBundle
        {
            Id = Guid.NewGuid().ToString("N"),
            Title = title,
            Description = description ?? string.Empty,
            ScenarioIds = scenarioIds,
            ImageId = imageId ?? string.Empty,
            Prices = prices ?? new List<BundlePrice>(),
            PriceCents = isFree ? 0 : (int)((prices?.FirstOrDefault()?.Value ?? 0) * 100),
            AgeGroupId = ageGroup ?? string.Empty
        };

        await _repository.AddAsync(bundle, ct);
        await _unitOfWork.SaveChangesAsync(ct);

        _logger.LogInformation("Created content bundle: {BundleId} - {Title}", bundle.Id, bundle.Title);
        return bundle;
    }
}

