using Microsoft.Extensions.Logging;
using Mystira.Application.Ports.Data;
using Mystira.Contracts.App.Requests.Contributors;
using Mystira.Domain.Models;

namespace Mystira.Application.UseCases.Contributors;

/// <summary>
/// Use case for setting contributors for a content bundle
/// </summary>
public class SetBundleContributorsUseCase
{
    private readonly IContentBundleRepository _bundleRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<SetBundleContributorsUseCase> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="SetBundleContributorsUseCase"/> class.
    /// </summary>
    /// <param name="bundleRepository">The content bundle repository.</param>
    /// <param name="unitOfWork">The unit of work for transaction management.</param>
    /// <param name="logger">The logger instance.</param>
    public SetBundleContributorsUseCase(
        IContentBundleRepository bundleRepository,
        IUnitOfWork unitOfWork,
        ILogger<SetBundleContributorsUseCase> logger)
    {
        _bundleRepository = bundleRepository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    /// <summary>
    /// Sets the contributors for a content bundle.
    /// </summary>
    /// <param name="bundleId">The bundle identifier.</param>
    /// <param name="request">The request containing contributor information.</param>
    /// <returns>The updated Story Protocol metadata.</returns>
    public async Task<ScenarioStoryProtocol> ExecuteAsync(string bundleId, SetContributorsRequest request)
    {
        // Get the bundle
        var bundle = await _bundleRepository.GetByIdAsync(bundleId);
        if (bundle == null)
        {
            throw new ArgumentException($"Content bundle not found: {bundleId}");
        }

        // Convert request to domain models
        var contributors = request.Contributors.Select(c => new Contributor
        {
            Id = Guid.NewGuid().ToString(),
            Name = c.Name,
            WalletAddress = c.WalletAddress ?? string.Empty,
            Role = (ContributorRole)(int)c.Role,
            ContributionPercentage = c.ContributionPercentage,
            Email = c.Email,
            Notes = c.Notes,
            CreatedAt = DateTime.UtcNow
        }).ToList();

        // Initialize Story Protocol metadata if it doesn't exist
        if (bundle.StoryProtocol == null)
        {
            bundle.StoryProtocol = new ScenarioStoryProtocol();
        }

        // Set the contributors
        bundle.StoryProtocol.Contributors = contributors;

        // Validate contributor splits
        if (!bundle.StoryProtocol.ValidateContributorSplits(out var errors))
        {
            var errorMessage = string.Join("; ", errors);
            _logger.LogWarning("Invalid contributor splits for bundle {BundleId}: {Errors}", bundleId, errorMessage);
            throw new ArgumentException($"Invalid contributor configuration: {errorMessage}");
        }

        // Update the bundle
        await _bundleRepository.UpdateAsync(bundle);

        try
        {
            await _unitOfWork.SaveChangesAsync();
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error saving contributors for bundle: {BundleId}", bundleId);
            throw;
        }

        _logger.LogInformation("Set {Count} contributors for bundle: {BundleId}", contributors.Count, bundleId);
        return bundle.StoryProtocol;
    }
}
