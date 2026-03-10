using Mystira.Domain.Models;
using Mystira.Domain.Enums;
using Mystira.Domain.ValueObjects;

namespace Mystira.App.Application.CQRS.Royalties.Commands;

/// <summary>
/// Command to pay royalties to an IP Asset
/// </summary>
public record PayRoyaltyCommand(
    string IpAssetId,
    decimal Amount,
    string? PayerReference = null) : ICommand<RoyaltyPaymentResult>;
