using Microsoft.Extensions.Logging;
using Mystira.App.Application.Ports.Data;
using System.Threading;

namespace Mystira.App.Application.UseCases.ContentBundles;

/// <summary>
/// Use case for deleting a content bundle
/// </summary>
public class DeleteContentBundleUseCase
{
    private readonly IContentBundleRepository _repository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<DeleteContentBundleUseCase> _logger;

    public DeleteContentBundleUseCase(
        IContentBundleRepository repository,
        IUnitOfWork unitOfWork,
        ILogger<DeleteContentBundleUseCase> logger)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<bool> ExecuteAsync(string bundleId, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(bundleId))
        {
            throw new ArgumentException("Bundle ID cannot be null or empty", nameof(bundleId));
        }

        var bundle = await _repository.GetByIdAsync(bundleId, ct);
        if (bundle == null)
        {
            _logger.LogWarning("Content bundle not found for deletion: {BundleId}", bundleId);
            return false;
        }

        await _repository.DeleteAsync(bundleId, ct);
        await _unitOfWork.SaveChangesAsync(ct);

        _logger.LogInformation("Deleted content bundle: {BundleId}", bundleId);
        return true;
    }
}

