using Mystira.Domain.Models;

namespace Mystira.Application.CQRS.Royalties.Commands;

/// <summary>
/// Command to pay royalties to an IP Asset
/// </summary>
public record PayRoyaltyCommand(
    string IpAssetId,
    decimal Amount,
    string? PayerReference = null) : ICommand<RoyaltyPaymentResult>;
