using Microsoft.Extensions.Logging;
using Mystira.Core.Ports.Data;
using Mystira.Shared.Exceptions;
using System.Threading;

namespace Mystira.Core.UseCases.ContentBundles;

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
            throw new ValidationException("bundleId", "bundleId is required");
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

