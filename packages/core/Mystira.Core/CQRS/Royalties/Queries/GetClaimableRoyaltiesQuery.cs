using Mystira.Domain.Models;
using Mystira.Domain.Enums;
using Mystira.Domain.ValueObjects;

namespace Mystira.Core.CQRS.Royalties.Queries;

/// <summary>
/// Query to get claimable royalties for an IP Asset
/// </summary>
public record GetClaimableRoyaltiesQuery(string IpAssetId) : IQuery<RoyaltyBalance>;
