using System.Globalization;
using System.Numerics;
using Grpc.Core;
using Grpc.Net.Client;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Mystira.Application.Configuration.StoryProtocol;
using Mystira.Application.Ports;
using Mystira.Chain.V1;
using Mystira.Domain.Enums;
using Mystira.Domain.Models;
using Polly;
using DomainContributor = Mystira.Domain.Models.Contributor;
using GrpcContributor = Mystira.Chain.V1.Contributor;
using GrpcPaymentStatus = Mystira.Chain.V1.PaymentStatus;

namespace Mystira.Infrastructure.StoryProtocol.Services.Grpc;

/// <summary>
/// gRPC client implementation of IStoryProtocolService.
/// Communicates with Mystira.Chain Python service for blockchain operations.
/// </summary>
public class GrpcStoryProtocolService : IStoryProtocolService, IDisposable
{
    private readonly ILogger<GrpcStoryProtocolService> _logger;
    private readonly ChainServiceOptions _options;
    private readonly GrpcChannel _channel;
    private readonly ChainService.ChainServiceClient _client;
    private readonly IAsyncPolicy _retryPolicy;
    private bool _disposed;

    /// <summary>
    /// Initializes a new instance of the <see cref="GrpcStoryProtocolService"/> class.
    /// </summary>
    /// <param name="logger">The logger instance.</param>
    /// <param name="options">The chain service configuration options.</param>
    public GrpcStoryProtocolService(
        ILogger<GrpcStoryProtocolService> logger,
        IOptions<ChainServiceOptions> options)
    {
        _logger = logger;
        _options = options.Value;

        // Configure gRPC channel
        var channelOptions = new GrpcChannelOptions
        {
            MaxRetryAttempts = _options.MaxRetryAttempts
        };

        _channel = GrpcChannel.ForAddress(_options.GrpcEndpoint, channelOptions);
        _client = new ChainService.ChainServiceClient(_channel);

        // Build retry policy with exponential backoff
        _retryPolicy = BuildRetryPolicy();

        _logger.LogInformation(
            "GrpcStoryProtocolService initialized. Endpoint: {Endpoint}, TLS: {UseTls}",
            _options.GrpcEndpoint, _options.UseTls);
    }

    /// <inheritdoc />
    /// <exception cref="ArgumentException">Thrown when contentId, contentTitle is null/empty or contributors is null/empty.</exception>
    public async Task<ScenarioStoryProtocol> RegisterIpAssetAsync(
        string contentId,
        string contentTitle,
        List<DomainContributor> contributors,
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
            "Registering IP Asset: ContentId={ContentId}, Title={Title}, Contributors={Count}",
            contentId, contentTitle, contributors.Count);

        var request = new RegisterIpAssetRequest
        {
            ContentId = contentId,
            ContentTitle = contentTitle,
            IdempotencyKey = $"register_{contentId}_{DateTime.UtcNow:yyyyMMddHHmmss}"
        };

        // Map contributors to proto messages
        foreach (var contributor in contributors)
        {
            request.Contributors.Add(new GrpcContributor
            {
                WalletAddress = contributor.WalletAddress ?? string.Empty,
                ContributorType = MapContributorRole(contributor.Role),
                ShareBasisPoints = (uint)(contributor.ContributionPercentage * 100), // Convert % to basis points
                DisplayName = contributor.Name
            });
        }

        if (!string.IsNullOrEmpty(metadataUri))
        {
            request.MetadataUri = metadataUri;
        }

        if (!string.IsNullOrEmpty(licenseTermsId))
        {
            request.LicenseTermsId = licenseTermsId;
        }

        var response = await ExecuteWithRetryAsync(async () =>
        {
            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(_options.TimeoutSeconds));
            return await _client.RegisterIpAssetAsync(
                request,
                headers: CreateAuthHeaders(),
                cancellationToken: cts.Token);
        });

        if (response.Status == IpAssetStatus.Failed)
        {
            _logger.LogError("IP Asset registration failed: {Error}", response.ErrorMessage);
            throw new InvalidOperationException($"IP Asset registration failed: {response.ErrorMessage}");
        }

        _logger.LogInformation(
            "IP Asset registered successfully: IpAssetId={IpAssetId}, TxHash={TxHash}",
            response.IpAssetId, response.RegistrationTxHash);

        return new ScenarioStoryProtocol
        {
            IpAssetId = response.IpAssetId,
            TransactionHash = response.RegistrationTxHash,
            RegisteredAt = response.RegisteredAt?.ToDateTime(),
            IsRegistered = response.Status == IpAssetStatus.Registered,
            LicenseTermsId = licenseTermsId,
            Contributors = contributors
        };
    }

    /// <inheritdoc />
    /// <exception cref="ArgumentException">Thrown when contentId is null or empty.</exception>
    public async Task<bool> IsRegisteredAsync(string contentId)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(contentId, nameof(contentId));

        _logger.LogDebug("Checking registration status for ContentId={ContentId}", contentId);

        var request = new IsRegisteredRequest { ContentId = contentId };

        var response = await ExecuteWithRetryAsync(async () =>
        {
            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(_options.TimeoutSeconds));
            return await _client.IsRegisteredAsync(
                request,
                headers: CreateAuthHeaders(),
                cancellationToken: cts.Token);
        });

        _logger.LogDebug(
            "Registration check complete: ContentId={ContentId}, IsRegistered={IsRegistered}",
            contentId, response.IsRegistered);

        return response.IsRegistered;
    }

    /// <inheritdoc />
    /// <exception cref="ArgumentException">Thrown when ipAssetId is null or empty.</exception>
    public async Task<ScenarioStoryProtocol?> GetRoyaltyConfigurationAsync(string ipAssetId)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(ipAssetId, nameof(ipAssetId));

        _logger.LogDebug("Getting royalty configuration for IpAssetId={IpAssetId}", ipAssetId);

        var request = new GetRoyaltyConfigurationRequest { IpAssetId = ipAssetId };

        var response = await ExecuteWithRetryAsync(async () =>
        {
            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(_options.TimeoutSeconds));
            return await _client.GetRoyaltyConfigurationAsync(
                request,
                headers: CreateAuthHeaders(),
                cancellationToken: cts.Token);
        });

        if (string.IsNullOrEmpty(response.IpAssetId))
        {
            return null;
        }

        var contributors = response.Recipients.Select(r => new DomainContributor
        {
            WalletAddress = r.WalletAddress,
            ContributionPercentage = r.ShareBasisPoints / 100m, // Convert basis points to %
            Name = r.Role
        }).ToList();

        return new ScenarioStoryProtocol
        {
            IpAssetId = response.IpAssetId,
            RoyaltyPolicyId = response.RoyaltyModuleId,
            Contributors = contributors,
            IsRegistered = true
        };
    }

    /// <inheritdoc />
    /// <exception cref="ArgumentException">Thrown when ipAssetId is null/empty or contributors is null/empty.</exception>
    public async Task<ScenarioStoryProtocol> UpdateRoyaltySplitAsync(string ipAssetId, List<DomainContributor> contributors)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(ipAssetId, nameof(ipAssetId));
        ArgumentNullException.ThrowIfNull(contributors, nameof(contributors));

        if (contributors.Count == 0)
        {
            throw new ArgumentException("At least one contributor is required.", nameof(contributors));
        }

        _logger.LogInformation(
            "Updating royalty split for IpAssetId={IpAssetId}, Contributors={Count}",
            ipAssetId, contributors.Count);

        var request = new UpdateRoyaltySplitRequest
        {
            IpAssetId = ipAssetId,
            IdempotencyKey = $"update_{ipAssetId}_{DateTime.UtcNow:yyyyMMddHHmmss}"
        };

        foreach (var contributor in contributors)
        {
            request.Contributors.Add(new GrpcContributor
            {
                WalletAddress = contributor.WalletAddress ?? string.Empty,
                ContributorType = MapContributorRole(contributor.Role),
                ShareBasisPoints = (uint)(contributor.ContributionPercentage * 100),
                DisplayName = contributor.Name
            });
        }

        var response = await ExecuteWithRetryAsync(async () =>
        {
            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(_options.TimeoutSeconds));
            return await _client.UpdateRoyaltySplitAsync(
                request,
                headers: CreateAuthHeaders(),
                cancellationToken: cts.Token);
        });

        if (!response.Success)
        {
            _logger.LogError("Royalty split update failed: {Error}", response.ErrorMessage);
            throw new InvalidOperationException($"Royalty split update failed: {response.ErrorMessage}");
        }

        _logger.LogInformation(
            "Royalty split updated successfully: IpAssetId={IpAssetId}, TxHash={TxHash}",
            ipAssetId, response.TransactionHash);

        return new ScenarioStoryProtocol
        {
            IpAssetId = ipAssetId,
            TransactionHash = response.TransactionHash,
            Contributors = contributors,
            IsRegistered = true
        };
    }

    /// <inheritdoc />
    /// <exception cref="ArgumentException">Thrown when ipAssetId is null/empty or amount is invalid.</exception>
    public async Task<RoyaltyPaymentResult> PayRoyaltyAsync(string ipAssetId, decimal amount, string? payerReference = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(ipAssetId, nameof(ipAssetId));

        if (amount <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(amount), amount, "Amount must be greater than zero.");
        }

        _logger.LogInformation(
            "Paying royalty: IpAssetId={IpAssetId}, Amount={Amount}, Reference={Reference}",
            ipAssetId, amount, payerReference);

        // Convert decimal to wei (18 decimals)
        var amountWei = ConvertToWei(amount);

        var request = new PayRoyaltiesRequest
        {
            IpAssetId = ipAssetId,
            AmountWei = amountWei,
            CurrencyToken = _options.WipTokenAddress,
            IdempotencyKey = $"pay_{ipAssetId}_{DateTime.UtcNow:yyyyMMddHHmmssfff}"
        };

        if (!string.IsNullOrEmpty(payerReference))
        {
            request.PayerReference = payerReference;
        }

        var response = await ExecuteWithRetryAsync(async () =>
        {
            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(_options.TimeoutSeconds));
            return await _client.PayRoyaltiesAsync(
                request,
                headers: CreateAuthHeaders(),
                cancellationToken: cts.Token);
        });

        var result = new RoyaltyPaymentResult
        {
            PaymentId = response.PaymentId ?? Guid.NewGuid().ToString(),
            IpAssetId = ipAssetId,
            TransactionHash = response.TransactionHash ?? string.Empty,
            Amount = amount,
            TokenAddress = _options.WipTokenAddress,
            PayerReference = payerReference,
            PaidAt = response.PaidAt?.ToDateTime() ?? DateTime.UtcNow,
            Success = response.Status == GrpcPaymentStatus.Confirmed,
            ErrorMessage = response.ErrorMessage
        };

        if (!result.Success)
        {
            _logger.LogError("Royalty payment failed: {Error}", response.ErrorMessage);
        }
        else
        {
            _logger.LogInformation(
                "Royalty payment successful: PaymentId={PaymentId}, TxHash={TxHash}",
                result.PaymentId, result.TransactionHash);
        }

        return result;
    }

    /// <inheritdoc />
    /// <exception cref="ArgumentException">Thrown when ipAssetId is null or empty.</exception>
    public async Task<RoyaltyBalance> GetClaimableRoyaltiesAsync(string ipAssetId)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(ipAssetId, nameof(ipAssetId));

        _logger.LogDebug("Getting claimable royalties for IpAssetId={IpAssetId}", ipAssetId);

        // Note: The proto expects wallet_address, but we need by IP Asset
        // We'll query with the IP Asset ID in the optional field
        var request = new GetClaimableRoyaltiesRequest
        {
            WalletAddress = string.Empty, // Will be populated from IP Asset's contributors
            IpAssetId = ipAssetId
        };

        var response = await ExecuteWithRetryAsync(async () =>
        {
            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(_options.TimeoutSeconds));
            return await _client.GetClaimableRoyaltiesAsync(
                request,
                headers: CreateAuthHeaders(),
                cancellationToken: cts.Token);
        });

        var balance = new RoyaltyBalance
        {
            IpAssetId = ipAssetId,
            TokenAddress = _options.WipTokenAddress,
            LastUpdated = response.LastUpdated?.ToDateTime() ?? DateTime.UtcNow
        };

        // Aggregate balances from response
        foreach (var b in response.Balances)
        {
            var claimable = ConvertFromWei(b.AmountWei);
            balance.TotalClaimable += claimable;
            balance.ContributorBalances.Add(new ContributorBalance
            {
                WalletAddress = response.WalletAddress,
                ClaimableAmount = claimable,
                // Include IP Asset ID from the balance for proper tracking
                ContributorId = b.IpAssetId
            });
        }

        return balance;
    }

    /// <inheritdoc />
    /// <exception cref="ArgumentException">Thrown when ipAssetId or contributorWallet is null or empty.</exception>
    public async Task<string> ClaimRoyaltiesAsync(string ipAssetId, string contributorWallet)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(ipAssetId, nameof(ipAssetId));
        ArgumentException.ThrowIfNullOrWhiteSpace(contributorWallet, nameof(contributorWallet));

        _logger.LogInformation(
            "Claiming royalties: IpAssetId={IpAssetId}, Wallet={Wallet}",
            ipAssetId, contributorWallet);

        var request = new ClaimRoyaltiesRequest
        {
            IpAssetId = ipAssetId,
            ContributorWallet = contributorWallet,
            CurrencyToken = _options.WipTokenAddress,
            IdempotencyKey = $"claim_{ipAssetId}_{contributorWallet}_{DateTime.UtcNow:yyyyMMddHHmmssfff}"
        };

        var response = await ExecuteWithRetryAsync(async () =>
        {
            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(_options.TimeoutSeconds));
            return await _client.ClaimRoyaltiesAsync(
                request,
                headers: CreateAuthHeaders(),
                cancellationToken: cts.Token);
        });

        if (response.Status == GrpcPaymentStatus.Failed)
        {
            _logger.LogError("Royalty claim failed: {Error}", response.ErrorMessage);
            throw new InvalidOperationException($"Royalty claim failed: {response.ErrorMessage}");
        }

        _logger.LogInformation(
            "Royalties claimed successfully: TxHash={TxHash}, Amount={Amount}",
            response.TransactionHash, response.AmountClaimedWei);

        return response.TransactionHash ?? string.Empty;
    }

    /// <summary>
    /// Performs a health check on the Chain service.
    /// </summary>
    /// <returns>True if healthy, false otherwise.</returns>
    public async Task<bool> IsHealthyAsync()
    {
        try
        {
            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(5));
            var response = await _client.HealthCheckAsync(
                new Google.Protobuf.WellKnownTypes.Empty(),
                headers: CreateAuthHeaders(),
                cancellationToken: cts.Token);

            return response.Status == HealthCheckResponse.Types.ServingStatus.Serving;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Health check failed for Chain service");
            return false;
        }
    }

    /// <summary>
    /// Gets service information from the Chain service.
    /// </summary>
    /// <returns>Service info response.</returns>
    public async Task<ServiceInfoResponse> GetServiceInfoAsync()
    {
        using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(5));
        return await _client.GetServiceInfoAsync(
            new Google.Protobuf.WellKnownTypes.Empty(),
            headers: CreateAuthHeaders(),
            cancellationToken: cts.Token);
    }

    private IAsyncPolicy BuildRetryPolicy()
    {
        if (!_options.EnableRetry)
        {
            return Policy.NoOpAsync();
        }

        return Policy
            .Handle<RpcException>(ex =>
                ex.StatusCode == StatusCode.Unavailable ||
                ex.StatusCode == StatusCode.DeadlineExceeded ||
                ex.StatusCode == StatusCode.ResourceExhausted)
            .WaitAndRetryAsync(
                _options.MaxRetryAttempts,
                retryAttempt =>
                    TimeSpan.FromMilliseconds(_options.RetryBaseDelayMs * Math.Pow(2, retryAttempt - 1)),
                onRetry: (exception, timeSpan, retryCount, context) =>
                {
                    _logger.LogWarning(
                        "Retry {RetryCount}/{MaxRetries} after {Delay}ms due to: {Error}",
                        retryCount, _options.MaxRetryAttempts, timeSpan.TotalMilliseconds, exception.Message);
                });
    }

    private async Task<T> ExecuteWithRetryAsync<T>(Func<Task<T>> operation)
    {
        return await _retryPolicy.ExecuteAsync(operation);
    }

    private Metadata CreateAuthHeaders()
    {
        var metadata = new Metadata();

        if (!string.IsNullOrEmpty(_options.ApiKey))
        {
            metadata.Add(_options.ApiKeyHeaderName, _options.ApiKey);
        }

        return metadata;
    }

    /// <summary>
    /// Maps domain ContributorRole to gRPC ContributorType.
    /// </summary>
    /// <param name="role">The domain contributor role.</param>
    /// <returns>The corresponding gRPC contributor type.</returns>
    private static ContributorType MapContributorRole(ContributorRole role)
    {
        return role switch
        {
            ContributorRole.Author => ContributorType.Author,
            ContributorRole.Writer => ContributorType.Author,
            ContributorRole.Artist => ContributorType.Artist,
            ContributorRole.Editor => ContributorType.Curator,
            ContributorRole.Designer => ContributorType.Other,
            ContributorRole.Composer => ContributorType.Other,
            ContributorRole.VoiceActor => ContributorType.Other,
            ContributorRole.Translator => ContributorType.Other,
            ContributorRole.SoundDesigner => ContributorType.Other,
            ContributorRole.GameDesigner => ContributorType.Other,
            ContributorRole.QualityAssurance => ContributorType.Other,
            ContributorRole.Other => ContributorType.Other,
            _ => ContributorType.Other
        };
    }

    /// <summary>
    /// Converts a decimal token amount to wei string representation.
    /// Uses BigInteger to handle amounts larger than long.MaxValue.
    /// </summary>
    /// <param name="amount">Token amount (e.g., 1.5 for 1.5 tokens).</param>
    /// <returns>Wei amount as string.</returns>
    /// <remarks>
    /// 1 token = 10^18 wei. Using BigInteger prevents overflow for large amounts.
    /// </remarks>
    internal static string ConvertToWei(decimal amount)
    {
        // Convert from token amount (with 18 decimals) to wei string
        // Use BigInteger to handle large values that exceed long.MaxValue
        const decimal weiPerToken = 1_000_000_000_000_000_000m;

        // Handle the conversion carefully to avoid decimal precision issues
        var wholeTokens = decimal.Truncate(amount);
        var fractionalTokens = amount - wholeTokens;

        // Calculate wei for whole tokens using BigInteger
        var wholeTokensWei = new BigInteger(wholeTokens) * new BigInteger(weiPerToken);

        // Calculate wei for fractional part (safe because it's < 10^18)
        var fractionalWei = (long)(fractionalTokens * weiPerToken);

        var totalWei = wholeTokensWei + fractionalWei;
        return totalWei.ToString();
    }

    /// <summary>
    /// Converts a wei string representation to decimal token amount.
    /// Uses BigInteger to handle amounts larger than long.MaxValue.
    /// </summary>
    /// <param name="weiString">Wei amount as string.</param>
    /// <returns>Token amount as decimal.</returns>
    internal static decimal ConvertFromWei(string weiString)
    {
        if (string.IsNullOrWhiteSpace(weiString))
        {
            return 0;
        }

        if (!BigInteger.TryParse(weiString, NumberStyles.None, CultureInfo.InvariantCulture, out var wei))
        {
            return 0;
        }

        const decimal weiPerToken = 1_000_000_000_000_000_000m;

        // For values that fit in decimal, convert directly
        // Decimal.MaxValue is approximately 7.9 x 10^28, so we're safe for reasonable token amounts
        if (wei <= new BigInteger(decimal.MaxValue))
        {
            return (decimal)wei / weiPerToken;
        }

        // For extremely large values, we need to handle carefully
        // This should be rare in practice
        var wholeTokens = wei / new BigInteger(weiPerToken);
        var remainderWei = wei % new BigInteger(weiPerToken);

        return (decimal)wholeTokens + (decimal)remainderWei / weiPerToken;
    }

    /// <inheritdoc />
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// Disposes the gRPC channel.
    /// </summary>
    /// <param name="disposing">True if disposing managed resources.</param>
    protected virtual void Dispose(bool disposing)
    {
        if (_disposed)
        {
            return;
        }

        if (disposing)
        {
            _channel.Dispose();
        }

        _disposed = true;
    }
}
