using Microsoft.Extensions.Logging;
using Mystira.App.Application.Ports;
using Mystira.App.Application.Ports.Data;
using Mystira.App.Domain.Exceptions;
using Mystira.Contracts.App.Requests.Contributors;
using Mystira.App.Domain.Models;
using System.Threading;

namespace Mystira.App.Application.UseCases.Contributors;

/// <summary>
/// Use case for registering a content bundle as an IP Asset on Story Protocol
/// </summary>
public class RegisterBundleIpAssetUseCase
{
    private readonly IContentBundleRepository _bundleRepository;
    private readonly IStoryProtocolService _storyProtocolService;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<RegisterBundleIpAssetUseCase> _logger;

    public RegisterBundleIpAssetUseCase(
        IContentBundleRepository bundleRepository,
        IStoryProtocolService storyProtocolService,
        IUnitOfWork unitOfWork,
        ILogger<RegisterBundleIpAssetUseCase> logger)
    {
        _bundleRepository = bundleRepository;
        _storyProtocolService = storyProtocolService;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<StoryProtocolMetadata> ExecuteAsync(string bundleId, RegisterIpAssetRequest request, CancellationToken ct = default)
    {
        // Get the bundle
        var bundle = await _bundleRepository.GetByIdAsync(bundleId, ct);
        if (bundle == null)
        {
            throw new NotFoundException("ContentBundle", bundleId);
        }

        // Check if already registered
        if (bundle.StoryProtocol?.IsRegistered ?? false)
        {
            throw new ConflictException("ContentBundle", $"Bundle {bundleId} is already registered on Story Protocol");
        }

        // Ensure contributors are set
        if (bundle.StoryProtocol == null || !bundle.StoryProtocol.Contributors.Any())
        {
            throw new BusinessRuleException("ContributorsRequired", "Contributors must be set before registering on Story Protocol");
        }

        // Validate contributor splits
        if (!bundle.StoryProtocol.ValidateContributorSplits(out var errors))
        {
            var errorMessage = string.Join("; ", errors);
            throw new ValidationException("contributors", $"Invalid contributor configuration: {errorMessage}");
        }

        // Register on Story Protocol
        var storyProtocolMetadata = await _storyProtocolService.RegisterIpAssetAsync(
            bundle.Id,
            bundle.Title,
            bundle.StoryProtocol.Contributors,
            request.MetadataUri,
            request.LicenseTermsId,
            ct);

        // Update the bundle with Story Protocol metadata
        bundle.StoryProtocol = storyProtocolMetadata;

        await _bundleRepository.UpdateAsync(bundle, ct);

        try
        {
            await _unitOfWork.SaveChangesAsync(ct);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error saving Story Protocol registration for bundle: {BundleId}", bundleId);
            throw;
        }

        _logger.LogInformation("Registered bundle {BundleId} as IP Asset: {IpAssetId}",
            bundleId, storyProtocolMetadata.IpAssetId);

        return storyProtocolMetadata;
    }
}
