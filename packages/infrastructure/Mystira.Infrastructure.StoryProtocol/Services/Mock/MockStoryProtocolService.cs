using System.Collections.Concurrent;
using Microsoft.Extensions.Logging;
using Mystira.Application.Ports;
using Mystira.Domain.Models;

namespace Mystira.Infrastructure.StoryProtocol.Services.Mock;

/// <summary>
/// Mock implementation of IStoryProtocolService for development and testing.
/// Simulates Story Protocol operations without actual blockchain calls.
/// </summary>
/// <remarks>
/// This implementation is thread-safe and can be used in concurrent scenarios.
/// All state is stored in memory and will be lost when the service is disposed.
/// </remarks>
public class MockStoryProtocolService : IStoryProtocolService
{
    private readonly ILogger<MockStoryProtocolService> _logger;
    private readonly ConcurrentDictionary<string, ScenarioStoryProtocol> _registeredAssets = new();
    private readonly ConcurrentDictionary<string, RoyaltyBalance> _balances = new();
    private readonly ConcurrentDictionary<string, List<RoyaltyPaymentResult>> _payments = new();
    private readonly object _paymentLock = new();

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
    /// <exception cref="ArgumentException">Thrown when contentId, contentTitle is null/empty or contributors is null/empty.</exception>
    public Task<ScenarioStoryProtocol> RegisterIpAssetAsync(
        string contentId,
        string contentTitle,
        List<Contributor> contributors,
        string? metadataUri = null,
        string? licenseTermsId = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(contentId, nameof(contentId));
        ArgumentException.ThrowIfNullOrWhiteSpace(contentTitle, nameof(contentTitle));
        ArgumentNullException.ThrowIfNull(contributors, nameof(contributors));

        if (contributors.Count == 0)
        {
            throw new ArgumentException("At least one contributor is required.", nameof(contributors));
        }

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
    /// <exception cref="ArgumentException">Thrown when contentId is null or empty.</exception>
    public Task<bool> IsRegisteredAsync(string contentId)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(contentId, nameof(contentId));

        var isRegistered = _registeredAssets.ContainsKey(contentId);
        _logger.LogDebug(
            "Mock: Checking registration: ContentId={ContentId}, IsRegistered={IsRegistered}",
            contentId, isRegistered);
        return Task.FromResult(isRegistered);
    }

    /// <inheritdoc />
    /// <exception cref="ArgumentException">Thrown when ipAssetId is null or empty.</exception>
    public Task<ScenarioStoryProtocol?> GetRoyaltyConfigurationAsync(string ipAssetId)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(ipAssetId, nameof(ipAssetId));

        _logger.LogDebug("Mock: Getting royalty configuration for IpAssetId={IpAssetId}", ipAssetId);

        var asset = _registeredAssets.Values.FirstOrDefault(a => a.IpAssetId == ipAssetId);
        return Task.FromResult(asset);
    }

    /// <inheritdoc />
    /// <exception cref="ArgumentException">Thrown when ipAssetId is null/empty or contributors is null/empty.</exception>
    public Task<ScenarioStoryProtocol> UpdateRoyaltySplitAsync(string ipAssetId, List<Contributor> contributors)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(ipAssetId, nameof(ipAssetId));
        ArgumentNullException.ThrowIfNull(contributors, nameof(contributors));

        if (contributors.Count == 0)
        {
            throw new ArgumentException("At least one contributor is required.", nameof(contributors));
        }

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
    /// <exception cref="ArgumentException">Thrown when ipAssetId is null/empty or amount is invalid.</exception>
    public Task<RoyaltyPaymentResult> PayRoyaltyAsync(string ipAssetId, decimal amount, string? payerReference = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(ipAssetId, nameof(ipAssetId));

        if (amount <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(amount), amount, "Amount must be greater than zero.");
        }

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

        // Update balances and create distributions (thread-safe)
        if (_balances.TryGetValue(ipAssetId, out var balance))
        {
            lock (_paymentLock)
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
        }

        // Track payment (thread-safe)
        // Use GetOrAdd to obtain the list, then lock and add the result
        // This avoids the concurrency issue where AddOrUpdate's update factory
        // can be called multiple times under contention
        var paymentList = _payments.GetOrAdd(ipAssetId, _ => new List<RoyaltyPaymentResult>());
        lock (_paymentLock)
        {
            paymentList.Add(result);
        }

        _logger.LogInformation(
            "Mock: Royalty paid: PaymentId={PaymentId}, TxHash={TxHash}, Distributions={Count}",
            paymentId, txHash, result.Distributions.Count);

        return Task.FromResult(result);
    }

    /// <inheritdoc />
    /// <exception cref="ArgumentException">Thrown when ipAssetId is null or empty.</exception>
    public Task<RoyaltyBalance> GetClaimableRoyaltiesAsync(string ipAssetId)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(ipAssetId, nameof(ipAssetId));

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
    /// <exception cref="ArgumentException">Thrown when ipAssetId or contributorWallet is null or empty.</exception>
    public Task<string> ClaimRoyaltiesAsync(string ipAssetId, string contributorWallet)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(ipAssetId, nameof(ipAssetId));
        ArgumentException.ThrowIfNullOrWhiteSpace(contributorWallet, nameof(contributorWallet));

        _logger.LogInformation(
            "Mock: Claiming royalties: IpAssetId={IpAssetId}, Wallet={Wallet}",
            ipAssetId, contributorWallet);

        var txHash = $"0x{Guid.NewGuid():N}";

        // Update balances (thread-safe)
        if (_balances.TryGetValue(ipAssetId, out var balance))
        {
            lock (_paymentLock)
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
