using Microsoft.Extensions.Logging;
using Mystira.Application.Ports.Data;
using Mystira.Domain.Models;

namespace Mystira.Application.UseCases.ContentBundles;

/// <summary>
/// Use case for retrieving all content bundles
/// </summary>
public class GetContentBundlesUseCase
{
    private readonly IContentBundleRepository _repository;
    private readonly ILogger<GetContentBundlesUseCase> _logger;

    /// <summary>Initializes a new instance of the <see cref="GetContentBundlesUseCase"/> class.</summary>
    /// <param name="repository">The content bundle repository.</param>
    /// <param name="logger">The logger.</param>
    public GetContentBundlesUseCase(
        IContentBundleRepository repository,
        ILogger<GetContentBundlesUseCase> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <summary>Retrieves all content bundles.</summary>
    /// <returns>A list of content bundles.</returns>
    public async Task<List<ContentBundle>> ExecuteAsync()
    {
        var bundles = await _repository.GetAllAsync();
        var bundleList = bundles.ToList();

        _logger.LogInformation("Retrieved {Count} content bundles", bundleList.Count);
        return bundleList;
    }
}

