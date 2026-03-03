using Microsoft.Extensions.Logging;
using Mystira.Application.Ports.Data;
using Mystira.Domain.Models;

namespace Mystira.Application.UseCases.ContentBundles;

/// <summary>
/// Use case for retrieving a content bundle by ID
/// </summary>
public class GetContentBundleUseCase
{
    private readonly IContentBundleRepository _repository;
    private readonly ILogger<GetContentBundleUseCase> _logger;

    /// <summary>Initializes a new instance of the <see cref="GetContentBundleUseCase"/> class.</summary>
    /// <param name="repository">The content bundle repository.</param>
    /// <param name="logger">The logger.</param>
    public GetContentBundleUseCase(
        IContentBundleRepository repository,
        ILogger<GetContentBundleUseCase> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <summary>Retrieves a content bundle by its identifier.</summary>
    /// <param name="bundleId">The bundle identifier.</param>
    /// <returns>The content bundle if found; otherwise, null.</returns>
    public async Task<ContentBundle?> ExecuteAsync(string bundleId)
    {
        if (string.IsNullOrWhiteSpace(bundleId))
        {
            throw new ArgumentException("Bundle ID cannot be null or empty", nameof(bundleId));
        }

        var bundle = await _repository.GetByIdAsync(bundleId);

        if (bundle == null)
        {
            _logger.LogWarning("Content bundle not found: {BundleId}", bundleId);
        }
        else
        {
            _logger.LogDebug("Retrieved content bundle: {BundleId}", bundleId);
        }

        return bundle;
    }
}

