using Microsoft.Extensions.Logging;
using Mystira.Application.Ports.Data;
using Mystira.Domain.Models;

namespace Mystira.Application.UseCases.ContentBundles;

/// <summary>
/// Use case for retrieving content bundles filtered by age group
/// </summary>
public class GetContentBundlesByAgeGroupUseCase
{
    private readonly IContentBundleRepository _repository;
    private readonly ILogger<GetContentBundlesByAgeGroupUseCase> _logger;

    /// <summary>Initializes a new instance of the <see cref="GetContentBundlesByAgeGroupUseCase"/> class.</summary>
    /// <param name="repository">The content bundle repository.</param>
    /// <param name="logger">The logger.</param>
    public GetContentBundlesByAgeGroupUseCase(
        IContentBundleRepository repository,
        ILogger<GetContentBundlesByAgeGroupUseCase> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <summary>Retrieves content bundles filtered by age group.</summary>
    /// <param name="ageGroup">The age group identifier.</param>
    /// <returns>A list of content bundles for the specified age group.</returns>
    public async Task<List<ContentBundle>> ExecuteAsync(string ageGroup)
    {
        if (string.IsNullOrWhiteSpace(ageGroup))
        {
            throw new ArgumentException("Age group cannot be null or empty", nameof(ageGroup));
        }

        var bundles = await _repository.GetByAgeGroupAsync(ageGroup);
        var bundleList = bundles.ToList();

        _logger.LogInformation("Retrieved {Count} content bundles for age group {AgeGroup}", bundleList.Count, ageGroup);
        return bundleList;
    }
}

