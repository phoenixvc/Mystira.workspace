using Microsoft.Extensions.Logging;
using Mystira.Application.Ports.Data;

namespace Mystira.Application.UseCases.ContentBundles;

/// <summary>
/// Use case for deleting a content bundle
/// </summary>
public class DeleteContentBundleUseCase
{
    private readonly IContentBundleRepository _repository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<DeleteContentBundleUseCase> _logger;

    /// <summary>Initializes a new instance of the <see cref="DeleteContentBundleUseCase"/> class.</summary>
    /// <param name="repository">The content bundle repository.</param>
    /// <param name="unitOfWork">The unit of work.</param>
    /// <param name="logger">The logger.</param>
    public DeleteContentBundleUseCase(
        IContentBundleRepository repository,
        IUnitOfWork unitOfWork,
        ILogger<DeleteContentBundleUseCase> logger)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    /// <summary>Deletes a content bundle by its identifier.</summary>
    /// <param name="bundleId">The bundle identifier.</param>
    /// <returns>True if the content bundle was deleted successfully; otherwise, false.</returns>
    public async Task<bool> ExecuteAsync(string bundleId)
    {
        if (string.IsNullOrWhiteSpace(bundleId))
        {
            throw new ArgumentException("Bundle ID cannot be null or empty", nameof(bundleId));
        }

        var bundle = await _repository.GetByIdAsync(bundleId);
        if (bundle == null)
        {
            _logger.LogWarning("Content bundle not found for deletion: {BundleId}", bundleId);
            return false;
        }

        await _repository.DeleteAsync(bundleId);
        await _unitOfWork.SaveChangesAsync();

        _logger.LogInformation("Deleted content bundle: {BundleId}", bundleId);
        return true;
    }
}

