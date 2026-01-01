using Mystira.Domain.Models;

namespace Mystira.Application.CQRS.Royalties.Queries;

/// <summary>
/// Query to get claimable royalties for an IP Asset
/// </summary>
/// <param name="IpAssetId">The unique identifier of the IP asset.</param>
public record GetClaimableRoyaltiesQuery(string IpAssetId) : IQuery<RoyaltyBalance>;
