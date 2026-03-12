using Grpc.Core;
using Grpc.Net.Client;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Mystira.App.Application.Configuration.StoryProtocol;
using Mystira.Application.Ports;
using Mystira.Domain.Models;
using Mystira.Domain.Enums;
using Mystira.Domain.ValueObjects;
using Mystira.App.Infrastructure.Chain.Protos;

namespace Mystira.App.Infrastructure.Chain.Services;

/// <summary>
/// gRPC adapter that connects IStoryProtocolService to the Mystira.Chain Python service.
/// Translates between .NET domain models and gRPC protobuf messages.
/// Proto: protos/mystira/chain/v1/chain_service.proto (canonical from Mystira.Chain repo)
/// </summary>
public class GrpcChainServiceAdapter : IStoryProtocolService, IAsyncDisposable
{
    private readonly GrpcChannel _channel;
    private readonly ChainService.ChainServiceClient _client;
    private readonly ChainServiceOptions _options;
    private readonly StoryProtocolOptions _spOptions;
    private readonly ILogger<GrpcChainServiceAdapter> _logger;

    public GrpcChainServiceAdapter(
        IOptions<ChainServiceOptions> options,
        IOptions<StoryProtocolOptions> spOptions,
        ILogger<GrpcChainServiceAdapter> logger)
    {
        _options = options.Value;
        _spOptions = spOptions.Value;
        _logger = logger;

        _channel = GrpcChannel.ForAddress(_options.GrpcEndpoint, new GrpcChannelOptions
        {
            MaxRetryAttempts = _options.MaxRetryAttempts
        });
        _client = new ChainService.ChainServiceClient(_channel);
    }

    public async Task<StoryProtocolMetadata> RegisterIpAssetAsync(
        string contentId,
        string contentTitle,
        List<Mystira.Domain.Models.Contributor> contributors,
        string? metadataUri = null,
        string? licenseTermsId = null,
        CancellationToken ct = default)
    {
        _logger.LogInformation("Registering IP Asset via Chain gRPC for content {ContentId}", contentId);

        if (!string.IsNullOrWhiteSpace(licenseTermsId))
        {
            _logger.LogWarning(
                "licenseTermsId '{LicenseTermsId}' provided for content {ContentId} but the Chain proto does not yet support license fields on RegisterIpAsset. " +
                "Use AttachLicenseTerms RPC separately.",
                licenseTermsId, contentId);
        }

        try
        {
            var grpcContributors = (contributors ?? new List<Mystira.Domain.Models.Contributor>()).Select(c => new Protos.Contributor
            {
                Name = c.Name,
                Address = c.WalletAddress,
                Type = MapContributorType(c.Role),
                SharePercentage = (double)c.ContributionPercentage
            });

            var metadata = new IpMetadata
            {
                Title = contentTitle,
                Description = $"Mystira content: {contentTitle}",
                AssetType = IpAssetType.Story
            };

            var deadline = DateTime.UtcNow.AddSeconds(_options.TimeoutSeconds);
            RegisterIpAssetResponse response;

            if (!string.IsNullOrWhiteSpace(metadataUri))
            {
                var requestWithMeta = new RegisterIpAssetWithMetadataRequest
                {
                    ContentId = contentId,
                    CollectionAddress = _spOptions.Contracts.SpgNft,
                    Metadata = metadata,
                    MetadataUri = metadataUri
                };
                requestWithMeta.Contributors.AddRange(grpcContributors);
                response = await _client.RegisterIpAssetWithMetadataAsync(requestWithMeta, deadline: deadline, cancellationToken: ct);
            }
            else
            {
                var request = new RegisterIpAssetRequest
                {
                    ContentId = contentId,
                    CollectionAddress = _spOptions.Contracts.SpgNft,
                    Metadata = metadata
                };
                request.Contributors.AddRange(grpcContributors);
                response = await _client.RegisterIpAssetAsync(request, deadline: deadline, cancellationToken: ct);
            }

            if (!response.Success)
            {
                _logger.LogError("Chain gRPC RegisterIpAsset failed for {ContentId}: {Error}", contentId, response.Error);
                throw new InvalidOperationException($"Story Protocol registration failed: {response.Error}");
            }

            _logger.LogInformation(
                "IP Asset registered: {IpAssetId} tx={TxHash} explorer={ExplorerUrl}",
                response.IpAssetId, response.TxHash, response.ExplorerUrl);

            return new StoryProtocolMetadata
            {
                IpAssetId = response.IpAssetId,
                RegistrationTxHash = response.TxHash,
                RegisteredAt = DateTime.UtcNow
            };
        }
        catch (OperationCanceledException) { throw; }
        catch (Exception ex) when (ex is not InvalidOperationException)
        {
            _logger.LogError(ex, "gRPC call failed for RegisterIpAssetAsync on content {ContentId}", contentId);
            throw new InvalidOperationException($"Story Protocol registration failed: {ex.Message}", ex);
        }
    }

    public async Task<bool> IsRegisteredAsync(string contentId, CancellationToken ct = default)
    {
        // Use GetIpAsset RPC to check registration status
        try
        {
            var response = await _client.GetIpAssetAsync(
                new GetIpAssetRequest { IpAssetId = contentId },
                deadline: DateTime.UtcNow.AddSeconds(_options.TimeoutSeconds),
                cancellationToken: ct);
            return response.Found;
        }
        catch (OperationCanceledException) { throw; }
        catch (RpcException ex)
        {
            _logger.LogDebug(ex, "IsRegisteredAsync gRPC call failed for {ContentId}, returning false", contentId);
            return false;
        }
    }

    public async Task<StoryProtocolMetadata?> GetRoyaltyConfigurationAsync(string ipAssetId, CancellationToken ct = default)
    {
        try
        {
            var response = await _client.GetIpAssetAsync(
                new GetIpAssetRequest { IpAssetId = ipAssetId },
                deadline: DateTime.UtcNow.AddSeconds(_options.TimeoutSeconds),
                cancellationToken: ct);

            if (!response.Found)
                return null;

            var metadata = new StoryProtocolMetadata
            {
                IpAssetId = response.IpAssetId,
                RegisteredAt = response.RegisteredAt?.ToDateTime()
            };

            return metadata;
        }
        catch (OperationCanceledException) { throw; }
        catch (RpcException ex)
        {
            _logger.LogWarning(ex, "GetRoyaltyConfigurationAsync gRPC call failed for {IpAssetId}", ipAssetId);
            return null;
        }
    }

    public Task<StoryProtocolMetadata> UpdateRoyaltySplitAsync(string ipAssetId, List<Mystira.Domain.Models.Contributor> contributors, CancellationToken ct = default)
    {
        // Royalty split updates require on-chain transactions not yet in Chain proto
        _logger.LogWarning("UpdateRoyaltySplitAsync not yet supported by Chain gRPC for {IpAssetId}", ipAssetId);
        return Task.FromResult(new StoryProtocolMetadata { IpAssetId = ipAssetId });
    }

    public Task<RoyaltyPaymentResult> PayRoyaltyAsync(string ipAssetId, decimal amount, string? payerReference = null, CancellationToken ct = default)
    {
        // Royalty payment RPC not yet in Chain proto
        _logger.LogWarning("PayRoyaltyAsync not yet supported by Chain gRPC for {IpAssetId}", ipAssetId);
        return Task.FromResult(new RoyaltyPaymentResult
        {
            IpAssetId = ipAssetId,
            Amount = amount,
            Success = false,
            ErrorMessage = "Royalty payments not yet supported via Chain gRPC"
        });
    }

    public Task<RoyaltyBalance> GetClaimableRoyaltiesAsync(string ipAssetId, CancellationToken ct = default)
    {
        _logger.LogDebug("GetClaimableRoyaltiesAsync not yet supported by Chain gRPC for {IpAssetId}", ipAssetId);
        return Task.FromResult(new RoyaltyBalance { IpAssetId = ipAssetId });
    }

    public Task<string> ClaimRoyaltiesAsync(string ipAssetId, string contributorWallet, CancellationToken ct = default)
    {
        _logger.LogWarning("ClaimRoyaltiesAsync not yet supported by Chain gRPC for {IpAssetId}", ipAssetId);
        return Task.FromResult(string.Empty);
    }

    public ValueTask DisposeAsync()
    {
        _channel?.Dispose();
        return ValueTask.CompletedTask;
    }

    /// <summary>
    /// Maps proto ContributorType back to domain ContributorRole.
    /// Used when reconstructing domain models from gRPC responses.
    /// </summary>
    private static ContributorRole MapContributorRole(ContributorType type) => type switch
    {
        ContributorType.Author => ContributorRole.Writer,
        ContributorType.Artist => ContributorRole.Artist,
        ContributorType.Editor => ContributorRole.Editor,
        ContributorType.Narrator => ContributorRole.VoiceActor,
        ContributorType.Composer => ContributorRole.MusicComposer,
        ContributorType.Producer => ContributorRole.SoundDesigner,
        ContributorType.Illustrator => ContributorRole.Artist,
        ContributorType.Translator => ContributorRole.Other,
        _ => ContributorRole.Other
    };

    /// <summary>
    /// Maps domain ContributorRole enum to proto ContributorType.
    /// Domain roles: Writer, Artist, VoiceActor, MusicComposer, SoundDesigner, Editor, GameDesigner, QualityAssurance, Other
    /// Proto types: Author, Artist, Editor, Narrator, Translator, Illustrator, Composer, Producer
    /// </summary>
    private static ContributorType MapContributorType(ContributorRole role) => role switch
    {
        ContributorRole.Writer => ContributorType.Author,
        ContributorRole.Artist => ContributorType.Artist,
        ContributorRole.Editor => ContributorType.Editor,
        ContributorRole.VoiceActor => ContributorType.Narrator,
        ContributorRole.MusicComposer => ContributorType.Composer,
        ContributorRole.SoundDesigner => ContributorType.Producer,
        ContributorRole.GameDesigner => ContributorType.Author,
        ContributorRole.QualityAssurance => ContributorType.Editor,
        ContributorRole.Other => ContributorType.Author,
        _ => ContributorType.Author
    };
}
