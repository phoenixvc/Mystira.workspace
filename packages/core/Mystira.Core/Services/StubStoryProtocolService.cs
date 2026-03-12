using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Mystira.Core.Configuration.StoryProtocol;
using Mystira.Core.Ports;
using Mystira.Domain.Models;
using Mystira.Domain.Enums;
using Mystira.Domain.ValueObjects;

namespace Mystira.Core.Services;

/// <summary>
/// Stub implementation of IStoryProtocolService for development/testing.
/// Returns mock data instead of making real blockchain calls.
/// Activated when StoryProtocolOptions.UseMockImplementation is true (default).
/// </summary>
public class StubStoryProtocolService : IStoryProtocolService
{
    private readonly ILogger<StubStoryProtocolService> _logger;

    public StubStoryProtocolService(ILogger<StubStoryProtocolService> logger)
    {
        _logger = logger;
    }

    public Task<StoryProtocolMetadata> RegisterIpAssetAsync(
        string contentId,
        string contentTitle,
        List<Contributor> contributors,
        string? metadataUri = null,
        string? licenseTermsId = null,
        CancellationToken ct = default)
    {
        _logger.LogWarning(
            "StoryProtocol stub: RegisterIpAssetAsync called for content {ContentId}. No blockchain transaction performed.",
            contentId);

        var metadata = new StoryProtocolMetadata
        {
            IpAssetId = $"stub-ip-{contentId}-{Guid.NewGuid():N}",
            RegistrationTxHash = $"0x{Guid.NewGuid():N}",
            RegisteredAt = DateTime.UtcNow,
            RoyaltyModuleId = $"stub-royalty-{Guid.NewGuid():N}"
        };

        return Task.FromResult(metadata);
    }

    public Task<bool> IsRegisteredAsync(string contentId, CancellationToken ct = default)
    {
        _logger.LogDebug("StoryProtocol stub: IsRegisteredAsync called for {ContentId}", contentId);
        return Task.FromResult(false);
    }

    public Task<StoryProtocolMetadata?> GetRoyaltyConfigurationAsync(string ipAssetId, CancellationToken ct = default)
    {
        _logger.LogDebug("StoryProtocol stub: GetRoyaltyConfigurationAsync called for {IpAssetId}", ipAssetId);
        return Task.FromResult<StoryProtocolMetadata?>(null);
    }

    public Task<StoryProtocolMetadata> UpdateRoyaltySplitAsync(string ipAssetId, List<Contributor> contributors, CancellationToken ct = default)
    {
        _logger.LogWarning(
            "StoryProtocol stub: UpdateRoyaltySplitAsync called for {IpAssetId}. No blockchain transaction performed.",
            ipAssetId);

        var metadata = new StoryProtocolMetadata
        {
            IpAssetId = ipAssetId,
            RegisteredAt = DateTime.UtcNow
        };

        return Task.FromResult(metadata);
    }

    public Task<RoyaltyPaymentResult> PayRoyaltyAsync(string ipAssetId, decimal amount, string? payerReference = null, CancellationToken ct = default)
    {
        _logger.LogWarning(
            "StoryProtocol stub: PayRoyaltyAsync called for {IpAssetId} with amount {Amount}. No blockchain transaction performed.",
            ipAssetId, amount);

        var result = new RoyaltyPaymentResult
        {
            PaymentId = $"stub-pay-{Guid.NewGuid():N}",
            IpAssetId = ipAssetId,
            TransactionHash = $"0x{Guid.NewGuid():N}",
            Amount = amount,
            PayerReference = payerReference,
            PaidAt = DateTime.UtcNow,
            Success = true
        };

        return Task.FromResult(result);
    }

    public Task<RoyaltyBalance> GetClaimableRoyaltiesAsync(string ipAssetId, CancellationToken ct = default)
    {
        _logger.LogDebug("StoryProtocol stub: GetClaimableRoyaltiesAsync called for {IpAssetId}", ipAssetId);

        var balance = new RoyaltyBalance
        {
            IpAssetId = ipAssetId,
            TotalClaimable = 0m,
            TotalClaimed = 0m,
            TotalReceived = 0m,
            LastUpdated = DateTime.UtcNow
        };

        return Task.FromResult(balance);
    }

    public Task<string> ClaimRoyaltiesAsync(string ipAssetId, string contributorWallet, CancellationToken ct = default)
    {
        _logger.LogWarning(
            "StoryProtocol stub: ClaimRoyaltiesAsync called for {IpAssetId} by wallet {Wallet}. No blockchain transaction performed.",
            ipAssetId, contributorWallet);

        return Task.FromResult($"0x{Guid.NewGuid():N}");
    }
}
