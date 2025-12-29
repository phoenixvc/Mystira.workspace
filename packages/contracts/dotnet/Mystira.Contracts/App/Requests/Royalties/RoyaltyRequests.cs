namespace Mystira.Contracts.App.Requests.Royalties;

/// <summary>
/// Request to pay royalties for an IP asset.
/// </summary>
public class PayRoyaltyRequest
{
    /// <summary>
    /// The unique identifier of the IP asset.
    /// </summary>
    public string IpAssetId { get; set; } = string.Empty;

    /// <summary>
    /// The amount to pay.
    /// </summary>
    public decimal Amount { get; set; }

    /// <summary>
    /// The currency for payment (default: ETH).
    /// </summary>
    public string Currency { get; set; } = "ETH";

    /// <summary>
    /// Optional reference identifier for the payer.
    /// </summary>
    public string? PayerReference { get; set; }
}

/// <summary>
/// Request to claim accumulated royalties.
/// </summary>
public class ClaimRoyaltiesRequest
{
    /// <summary>
    /// The unique identifier of the IP asset.
    /// </summary>
    public string IpAssetId { get; set; } = string.Empty;

    /// <summary>
    /// The wallet address to receive the royalties.
    /// </summary>
    public string WalletAddress { get; set; } = string.Empty;

    /// <summary>
    /// The wallet address of the contributor claiming royalties.
    /// </summary>
    public string ContributorWallet { get; set; } = string.Empty;
}
