using Microsoft.Extensions.Logging;
using Mystira.Application.Ports;
using Mystira.Domain.Models;

namespace Mystira.Infrastructure.StoryProtocol.Services.Mock;

/// <summary>
/// Mock implementation of IStoryProtocolService for development and testing.
/// Simulates Story Protocol operations without actual blockchain calls.
/// </summary>
public class MockStoryProtocolService : IStoryProtocolService
{
    private readonly ILogger<MockStoryProtocolService> _logger;
    private readonly Dictionary<string, ScenarioStoryProtocol> _registeredAssets = new();
    private readonly Dictionary<string, RoyaltyBalance> _balances = new();
    private readonly Dictionary<string, List<RoyaltyPaymentResult>> _payments = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="MockStoryProtocolService"/> class.
    /// </summary>
    /// <param name="logger">The logger instance.</param>
    public MockStoryProtocolService(ILogger<MockStoryProtocolService> logger)
    {
        _logger = logger;
        _logger.LogInformation("MockStoryProtocolService initialized - blockchain operations will be simulated");
    }

    /// <inheritdoc />
    public Task<ScenarioStoryProtocol> RegisterIpAssetAsync(
        string contentId,
        string contentTitle,
        List<Contributor> contributors,
        string? metadataUri = null,
        string? licenseTermsId = null)
    {
        _logger.LogInformation(
            "Mock: Registering IP Asset: ContentId={ContentId}, Title={Title}, Contributors={Count}",
            contentId, contentTitle, contributors.Count);

        // Simulate blockchain delay
        var ipAssetId = $"0x{Guid.NewGuid():N}";
        var txHash = $"0x{Guid.NewGuid():N}";

        var result = new ScenarioStoryProtocol
        {
            IpAssetId = ipAssetId,
            TransactionHash = txHash,
            RegisteredAt = DateTime.UtcNow,
            IsRegistered = true,
            LicenseTermsId = licenseTermsId ?? "mock-license-001",
            RoyaltyPolicyId = $"0x{Guid.NewGuid():N}",
            Contributors = contributors
        };

        _registeredAssets[contentId] = result;

        // Initialize balance tracking
        _balances[ipAssetId] = new RoyaltyBalance
        {
            IpAssetId = ipAssetId,
            TotalClaimable = 0,
            TotalClaimed = 0,
            TotalReceived = 0,
            TokenAddress = "0x1514000000000000000000000000000000000000",
            ContributorBalances = contributors.Select(c => new ContributorBalance
            {
                ContributorId = c.Id,
                WalletAddress = c.WalletAddress ?? string.Empty,
                ContributorName = c.Name,
                SharePercentage = c.ContributionPercentage,
                ClaimableAmount = 0,
                ClaimedAmount = 0,
                TotalEarned = 0
            }).ToList(),
            LastUpdated = DateTime.UtcNow
        };

        _logger.LogInformation(
            "Mock: IP Asset registered: IpAssetId={IpAssetId}, TxHash={TxHash}",
            ipAssetId, txHash);

        return Task.FromResult(result);
    }

    /// <inheritdoc />
    public Task<bool> IsRegisteredAsync(string contentId)
    {
        var isRegistered = _registeredAssets.ContainsKey(contentId);
        _logger.LogDebug(
            "Mock: Checking registration: ContentId={ContentId}, IsRegistered={IsRegistered}",
            contentId, isRegistered);
        return Task.FromResult(isRegistered);
    }

    /// <inheritdoc />
    public Task<ScenarioStoryProtocol?> GetRoyaltyConfigurationAsync(string ipAssetId)
    {
        _logger.LogDebug("Mock: Getting royalty configuration for IpAssetId={IpAssetId}", ipAssetId);

        var asset = _registeredAssets.Values.FirstOrDefault(a => a.IpAssetId == ipAssetId);
        return Task.FromResult(asset);
    }

    /// <inheritdoc />
    public Task<ScenarioStoryProtocol> UpdateRoyaltySplitAsync(string ipAssetId, List<Contributor> contributors)
    {
        _logger.LogInformation(
            "Mock: Updating royalty split for IpAssetId={IpAssetId}, Contributors={Count}",
            ipAssetId, contributors.Count);

        // Find the content ID for this IP Asset
        var entry = _registeredAssets.FirstOrDefault(kvp => kvp.Value.IpAssetId == ipAssetId);
        if (entry.Key == null)
        {
            throw new InvalidOperationException($"IP Asset {ipAssetId} not found");
        }

        var updated = new ScenarioStoryProtocol
        {
            IpAssetId = ipAssetId,
            TransactionHash = $"0x{Guid.NewGuid():N}",
            RegisteredAt = entry.Value.RegisteredAt,
            IsRegistered = true,
            LicenseTermsId = entry.Value.LicenseTermsId,
            RoyaltyPolicyId = entry.Value.RoyaltyPolicyId,
            Contributors = contributors
        };

        _registeredAssets[entry.Key] = updated;

        _logger.LogInformation(
            "Mock: Royalty split updated: IpAssetId={IpAssetId}, TxHash={TxHash}",
            ipAssetId, updated.TransactionHash);

        return Task.FromResult(updated);
    }

    /// <inheritdoc />
    public Task<RoyaltyPaymentResult> PayRoyaltyAsync(string ipAssetId, decimal amount, string? payerReference = null)
    {
        _logger.LogInformation(
            "Mock: Paying royalty: IpAssetId={IpAssetId}, Amount={Amount}, Reference={Reference}",
            ipAssetId, amount, payerReference);

        var paymentId = $"pay_{Guid.NewGuid():N}";
        var txHash = $"0x{Guid.NewGuid():N}";

        var result = new RoyaltyPaymentResult
        {
            PaymentId = paymentId,
            IpAssetId = ipAssetId,
            TransactionHash = txHash,
            Amount = amount,
            TokenAddress = "0x1514000000000000000000000000000000000000",
            PayerReference = payerReference,
            PaidAt = DateTime.UtcNow,
            Success = true,
            Distributions = new List<RoyaltyDistribution>()
        };

        // Update balances and create distributions
        if (_balances.TryGetValue(ipAssetId, out var balance))
        {
            balance.TotalReceived += amount;

            foreach (var cb in balance.ContributorBalances)
            {
                var distributedAmount = amount * (cb.SharePercentage / 100m);
                cb.ClaimableAmount += distributedAmount;
                cb.TotalEarned += distributedAmount;

                result.Distributions.Add(new RoyaltyDistribution
                {
                    ContributorId = cb.ContributorId,
                    WalletAddress = cb.WalletAddress,
                    ContributorName = cb.ContributorName,
                    SharePercentage = cb.SharePercentage,
                    Amount = distributedAmount
                });
            }

            balance.TotalClaimable = balance.ContributorBalances.Sum(cb => cb.ClaimableAmount);
            balance.LastUpdated = DateTime.UtcNow;
        }

        // Track payment
        if (!_payments.ContainsKey(ipAssetId))
        {
            _payments[ipAssetId] = new List<RoyaltyPaymentResult>();
        }
        _payments[ipAssetId].Add(result);

        _logger.LogInformation(
            "Mock: Royalty paid: PaymentId={PaymentId}, TxHash={TxHash}, Distributions={Count}",
            paymentId, txHash, result.Distributions.Count);

        return Task.FromResult(result);
    }

    /// <inheritdoc />
    public Task<RoyaltyBalance> GetClaimableRoyaltiesAsync(string ipAssetId)
    {
        _logger.LogDebug("Mock: Getting claimable royalties for IpAssetId={IpAssetId}", ipAssetId);

        if (_balances.TryGetValue(ipAssetId, out var balance))
        {
            return Task.FromResult(balance);
        }

        // Return empty balance for unregistered assets
        return Task.FromResult(new RoyaltyBalance
        {
            IpAssetId = ipAssetId,
            TotalClaimable = 0,
            TotalClaimed = 0,
            TotalReceived = 0,
            TokenAddress = "0x1514000000000000000000000000000000000000",
            ContributorBalances = new List<ContributorBalance>(),
            LastUpdated = DateTime.UtcNow
        });
    }

    /// <inheritdoc />
    public Task<string> ClaimRoyaltiesAsync(string ipAssetId, string contributorWallet)
    {
        _logger.LogInformation(
            "Mock: Claiming royalties: IpAssetId={IpAssetId}, Wallet={Wallet}",
            ipAssetId, contributorWallet);

        var txHash = $"0x{Guid.NewGuid():N}";

        // Update balances
        if (_balances.TryGetValue(ipAssetId, out var balance))
        {
            var contributorBalance = balance.ContributorBalances
                .FirstOrDefault(cb => cb.WalletAddress == contributorWallet);

            if (contributorBalance != null)
            {
                contributorBalance.ClaimedAmount += contributorBalance.ClaimableAmount;
                balance.TotalClaimed += contributorBalance.ClaimableAmount;
                balance.TotalClaimable -= contributorBalance.ClaimableAmount;
                contributorBalance.ClaimableAmount = 0;
                balance.LastUpdated = DateTime.UtcNow;

                _logger.LogInformation(
                    "Mock: Royalties claimed: TxHash={TxHash}, TotalClaimed={TotalClaimed}",
                    txHash, contributorBalance.ClaimedAmount);
            }
        }

        return Task.FromResult(txHash);
    }

    /// <summary>
    /// Performs a health check on the mock service (always healthy).
    /// </summary>
    /// <returns>Always returns true.</returns>
    public Task<bool> IsHealthyAsync()
    {
        _logger.LogDebug("Mock: Health check - always healthy");
        return Task.FromResult(true);
    }
}
