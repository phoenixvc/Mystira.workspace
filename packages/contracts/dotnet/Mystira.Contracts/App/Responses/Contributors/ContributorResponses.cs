namespace Mystira.Contracts.App.Responses.Contributors;

/// <summary>
/// Response containing Story Protocol registration information.
/// </summary>
public record StoryProtocolResponse
{
    /// <summary>
    /// The unique identifier of the Story Protocol registration.
    /// </summary>
    public string Id { get; set; } = string.Empty;

    /// <summary>
    /// The IP asset identifier on Story Protocol.
    /// </summary>
    public string? IpAssetId { get; set; }

    /// <summary>
    /// The license terms identifier.
    /// </summary>
    public string? LicenseTermsId { get; set; }

    /// <summary>
    /// The blockchain transaction hash for registration.
    /// </summary>
    public string? TransactionHash { get; set; }

    /// <summary>
    /// The registration status.
    /// </summary>
    public string Status { get; set; } = string.Empty;

    /// <summary>
    /// When the registration was created.
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// When the registration was last updated.
    /// </summary>
    public DateTime? UpdatedAt { get; set; }

    /// <summary>
    /// The NFT contract address if minted.
    /// </summary>
    public string? NftContractAddress { get; set; }

    /// <summary>
    /// The NFT token ID if minted.
    /// </summary>
    public string? NftTokenId { get; set; }

    /// <summary>
    /// Metadata URI for the IP asset.
    /// </summary>
    public string? MetadataUri { get; set; }
}

/// <summary>
/// Response containing contributor information.
/// </summary>
public record ContributorResponse
{
    /// <summary>
    /// The unique identifier of the contributor.
    /// </summary>
    public string Id { get; set; } = string.Empty;

    /// <summary>
    /// The display name of the contributor.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// The email address of the contributor.
    /// </summary>
    public string? Email { get; set; }

    /// <summary>
    /// The role of the contributor (e.g., "author", "illustrator", "editor").
    /// </summary>
    public string Role { get; set; } = string.Empty;

    /// <summary>
    /// The share percentage for royalty distribution (0-100).
    /// </summary>
    public decimal SharePercentage { get; set; }

    /// <summary>
    /// The wallet address for receiving royalties.
    /// </summary>
    public string? WalletAddress { get; set; }

    /// <summary>
    /// Whether the contributor has been verified.
    /// </summary>
    public bool IsVerified { get; set; }

    /// <summary>
    /// Biography or description of the contributor.
    /// </summary>
    public string? Bio { get; set; }

    /// <summary>
    /// URL to the contributor's profile image.
    /// </summary>
    public string? ProfileImageUrl { get; set; }

    /// <summary>
    /// Social media links for the contributor.
    /// </summary>
    public Dictionary<string, string>? SocialLinks { get; set; }

    /// <summary>
    /// When the contributor record was created.
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// Count of scenarios this contributor has worked on.
    /// </summary>
    public int ScenarioCount { get; set; }

    /// <summary>
    /// Total royalties earned by this contributor.
    /// </summary>
    public decimal TotalRoyaltiesEarned { get; set; }
}
