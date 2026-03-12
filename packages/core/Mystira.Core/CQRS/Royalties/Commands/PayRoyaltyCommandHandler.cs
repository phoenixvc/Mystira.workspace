using Microsoft.Extensions.Logging;
using Mystira.Core.Ports;
using Mystira.Domain.Models;
using Mystira.Domain.Enums;
using Mystira.Domain.ValueObjects;
using Mystira.Shared.Exceptions;

namespace Mystira.Core.CQRS.Royalties.Commands;

/// <summary>
/// Wolverine message handler for paying royalties to IP assets.
/// </summary>
public static class PayRoyaltyCommandHandler
{
    public static async Task<RoyaltyPaymentResult> Handle(
        PayRoyaltyCommand request,
        IStoryProtocolService storyProtocolService,
        ILogger<PayRoyaltyCommand> logger,
        CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(request.IpAssetId))
        {
            throw new ValidationException("ipAssetId", "IP Asset ID cannot be null or empty");
        }

        if (request.Amount <= 0)
        {
            throw new ValidationException("amount", "Amount must be greater than zero");
        }

        logger.LogInformation(
            "Processing royalty payment of {Amount} to IP Asset: {IpAssetId}",
            request.Amount, request.IpAssetId);

        return await storyProtocolService.PayRoyaltyAsync(
            request.IpAssetId,
            request.Amount,
            request.PayerReference);
    }
}
