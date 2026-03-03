using Microsoft.Extensions.Logging;
using Mystira.Application.Ports;
using Mystira.Domain.Models;

namespace Mystira.Application.CQRS.Royalties.Commands;

/// <summary>
/// Wolverine message handler for paying royalties to IP assets.
/// </summary>
public static class PayRoyaltyCommandHandler
{
    /// <summary>
    /// Handles the PayRoyaltyCommand.
    /// </summary>
    /// <param name="request">The command to handle.</param>
    /// <param name="storyProtocolService">The Story Protocol service.</param>
    /// <param name="logger">The logger instance.</param>
    /// <param name="ct">The cancellation token.</param>
    /// <returns>The royalty payment result.</returns>
    public static async Task<RoyaltyPaymentResult> Handle(
        PayRoyaltyCommand request,
        IStoryProtocolService storyProtocolService,
        ILogger<PayRoyaltyCommand> logger,
        CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(request.IpAssetId))
        {
            throw new ArgumentException("IP Asset ID cannot be null or empty", nameof(request.IpAssetId));
        }

        if (request.Amount <= 0)
        {
            throw new ArgumentException("Amount must be greater than zero", nameof(request.Amount));
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
