namespace Mystira.Contracts.App.Responses.Attribution;

/// <summary>
/// Response containing attribution information for content.
/// </summary>
public record ContentAttributionResponse
{
    /// <summary>
    /// The unique identifier of the content.
    /// </summary>
    public string ContentId { get; set; } = string.Empty;

    /// <summary>
    /// The title of the content.
    /// </summary>
    public string ContentTitle { get; set; } = string.Empty;

    /// <summary>
    /// Indicates if the content has been registered as intellectual property.
    /// </summary>
    public bool IsIpRegistered { get; set; }

    /// <summary>
    /// The IP asset identifier if registered.
    /// </summary>
    public string? IpAssetId { get; set; }

    /// <summary>
    /// The date and time when the IP was registered.
    /// </summary>
    public DateTime? RegisteredAt { get; set; }

    /// <summary>
    /// The list of creator credits for this content.
    /// </summary>
    public List<CreatorCredit> Credits { get; set; } = new();
}

/// <summary>
/// Represents a creator credit for attribution.
/// </summary>
public record CreatorCredit
{
    /// <summary>
    /// The name of the creator.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// The role of the creator (e.g., Author, Artist, Editor).
    /// </summary>
    public string Role { get; set; } = string.Empty;

    /// <summary>
    /// The percentage of contribution for revenue sharing.
    /// </summary>
    public decimal? ContributionPercentage { get; set; }
}

/// <summary>
/// Response containing IP verification status information.
/// </summary>
public record IpVerificationResponse
{
    /// <summary>
    /// The unique identifier of the content.
    /// </summary>
    public string ContentId { get; set; } = string.Empty;

    /// <summary>
    /// The title of the content.
    /// </summary>
    public string ContentTitle { get; set; } = string.Empty;

    /// <summary>
    /// Indicates if the content is registered as intellectual property.
    /// </summary>
    public bool IsRegistered { get; set; }

    /// <summary>
    /// The IP asset identifier if registered.
    /// </summary>
    public string? IpAssetId { get; set; }

    /// <summary>
    /// The date and time when the IP was registered.
    /// </summary>
    public DateTime? RegisteredAt { get; set; }

    /// <summary>
    /// The transaction hash of the registration on the blockchain.
    /// </summary>
    public string? RegistrationTxHash { get; set; }

    /// <summary>
    /// The number of contributors associated with this content.
    /// </summary>
    public int ContributorCount { get; set; }
}
