using Microsoft.Extensions.Logging;
using Mystira.Application.Ports;

namespace Mystira.Application.CQRS.Royalties.Commands;

/// <summary>
/// Wolverine message handler for claiming royalties from IP assets.
/// </summary>
public static class ClaimRoyaltiesCommandHandler
{
    /// <summary>
    /// Handles the ClaimRoyaltiesCommand.
    /// </summary>
    /// <param name="request">The command to handle.</param>
    /// <param name="storyProtocolService">The Story Protocol service.</param>
    /// <param name="logger">The logger instance.</param>
    /// <param name="ct">The cancellation token.</param>
    /// <returns>The transaction hash of the royalty claim.</returns>
    public static async Task<string> Handle(
        ClaimRoyaltiesCommand request,
        IStoryProtocolService storyProtocolService,
        ILogger<ClaimRoyaltiesCommand> logger,
        CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(request.IpAssetId))
        {
            throw new ArgumentException("IP Asset ID cannot be null or empty", nameof(request.IpAssetId));
        }

        if (string.IsNullOrWhiteSpace(request.ContributorWallet))
        {
            throw new ArgumentException("Contributor wallet address cannot be null or empty", nameof(request.ContributorWallet));
        }

        logger.LogInformation(
            "Claiming royalties for wallet {Wallet} from IP Asset: {IpAssetId}",
            request.ContributorWallet, request.IpAssetId);

        return await storyProtocolService.ClaimRoyaltiesAsync(
            request.IpAssetId,
            request.ContributorWallet);
    }
}
