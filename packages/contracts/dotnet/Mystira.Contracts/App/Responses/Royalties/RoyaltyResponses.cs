namespace Mystira.Contracts.App.Responses.Royalties;

/// <summary>
/// Response for claim royalties operation.
/// </summary>
public record ClaimRoyaltiesResponse
{
    /// <summary>
    /// The IP Asset ID.
    /// </summary>
    public string IpAssetId { get; set; } = string.Empty;

    /// <summary>
    /// The wallet that claimed the royalties.
    /// </summary>
    public string ContributorWallet { get; set; } = string.Empty;

    /// <summary>
    /// Transaction hash of the claim.
    /// </summary>
    public string TransactionHash { get; set; } = string.Empty;

    /// <summary>
    /// When the claim was processed.
    /// </summary>
    public DateTime ClaimedAt { get; set; }
}
