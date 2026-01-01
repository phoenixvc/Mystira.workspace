using Microsoft.Extensions.Logging;
using Mystira.Application.Ports;
using Mystira.Domain.Models;

namespace Mystira.Application.CQRS.Royalties.Queries;

/// <summary>
/// Wolverine message handler for retrieving claimable royalties for an IP asset.
/// </summary>
public static class GetClaimableRoyaltiesQueryHandler
{
    /// <summary>
    /// Handles the GetClaimableRoyaltiesQuery.
    /// </summary>
    /// <param name="request">The query to handle.</param>
    /// <param name="storyProtocolService">The Story Protocol service.</param>
    /// <param name="logger">The logger instance.</param>
    /// <param name="ct">The cancellation token.</param>
    /// <returns>The royalty balance information.</returns>
    public static async Task<RoyaltyBalance> Handle(
        GetClaimableRoyaltiesQuery request,
        IStoryProtocolService storyProtocolService,
        ILogger<GetClaimableRoyaltiesQuery> logger,
        CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(request.IpAssetId))
        {
            throw new ArgumentException("IP Asset ID cannot be null or empty", nameof(request.IpAssetId));
        }

        logger.LogInformation("Getting claimable royalties for IP Asset: {IpAssetId}", request.IpAssetId);

        return await storyProtocolService.GetClaimableRoyaltiesAsync(request.IpAssetId);
    }
}
