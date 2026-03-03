using Mystira.Domain.Models;

namespace Mystira.Application.CQRS.Royalties.Commands;

/// <summary>
/// Command to pay royalties to an IP Asset
/// </summary>
/// <param name="IpAssetId">The unique identifier of the IP asset receiving royalties.</param>
/// <param name="Amount">The amount of royalties to pay.</param>
/// <param name="PayerReference">Optional reference information from the payer.</param>
public record PayRoyaltyCommand(
    string IpAssetId,
    decimal Amount,
    string? PayerReference = null) : ICommand<RoyaltyPaymentResult>;
