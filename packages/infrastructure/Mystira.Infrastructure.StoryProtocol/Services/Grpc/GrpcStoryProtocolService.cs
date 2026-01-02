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
using Polly.Retry;

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
    private readonly AsyncRetryPolicy _retryPolicy;
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
    public async Task<ScenarioStoryProtocol> RegisterIpAssetAsync(
        string contentId,
        string contentTitle,
        List<Contributor> contributors,
        string? metadataUri = null,
        string? licenseTermsId = null)
    {
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
            request.Contributors.Add(new Chain.V1.Contributor
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
    public async Task<bool> IsRegisteredAsync(string contentId)
    {
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
    public async Task<ScenarioStoryProtocol?> GetRoyaltyConfigurationAsync(string ipAssetId)
    {
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

        var contributors = response.Recipients.Select(r => new Contributor
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
    public async Task<ScenarioStoryProtocol> UpdateRoyaltySplitAsync(string ipAssetId, List<Contributor> contributors)
    {
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
            request.Contributors.Add(new Chain.V1.Contributor
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
    public async Task<RoyaltyPaymentResult> PayRoyaltyAsync(string ipAssetId, decimal amount, string? payerReference = null)
    {
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
            Success = response.Status == PaymentStatus.Confirmed,
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
    public async Task<RoyaltyBalance> GetClaimableRoyaltiesAsync(string ipAssetId)
    {
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
                ClaimableAmount = claimable
            });
        }

        return balance;
    }

    /// <inheritdoc />
    public async Task<string> ClaimRoyaltiesAsync(string ipAssetId, string contributorWallet)
    {
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

        if (response.Status == PaymentStatus.Failed)
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

    private AsyncRetryPolicy BuildRetryPolicy()
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

    private static ContributorType MapContributorRole(ContributorRole role)
    {
        return role switch
        {
            ContributorRole.Author => ContributorType.Author,
            ContributorRole.Artist => ContributorType.Artist,
            ContributorRole.Editor => ContributorType.Curator,
            ContributorRole.Writer => ContributorType.Author,
            _ => ContributorType.Other
        };
    }

    private static string ConvertToWei(decimal amount)
    {
        // Convert from token amount (with 18 decimals) to wei string
        var wei = amount * 1_000_000_000_000_000_000m;
        return ((long)wei).ToString();
    }

    private static decimal ConvertFromWei(string weiString)
    {
        if (!long.TryParse(weiString, out var wei))
        {
            return 0;
        }
        return wei / 1_000_000_000_000_000_000m;
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
