using Mystira.Contracts.App.Enums;

namespace Mystira.Contracts.App.Requests.Contributors;

/// <summary>
/// Request to register an intellectual property asset.
/// </summary>
public record RegisterIpAssetRequest
{
    /// <summary>
    /// The URI pointing to the metadata for this IP asset.
    /// </summary>
    public string MetadataUri { get; set; } = string.Empty;

    /// <summary>
    /// The hash of the metadata content for verification.
    /// </summary>
    public string MetadataHash { get; set; } = string.Empty;

    /// <summary>
    /// Optional license terms identifier for this IP asset.
    /// </summary>
    public string? LicenseTermsId { get; set; }
}

/// <summary>
/// Request to set contributors for a content item.
/// </summary>
public record SetContributorsRequest
{
    /// <summary>
    /// The list of contributors to assign.
    /// </summary>
    public List<ContributorRequest> Contributors { get; set; } = new();
}

/// <summary>
/// Request representing a single contributor.
/// </summary>
public record ContributorRequest
{
    /// <summary>
    /// The name of the contributor.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// The wallet address of the contributor for payments.
    /// </summary>
    public string? WalletAddress { get; set; }

    /// <summary>
    /// The role of the contributor.
    /// </summary>
    public ContributorRole Role { get; set; }

    /// <summary>
    /// The percentage of contribution for revenue sharing.
    /// </summary>
    public decimal ContributionPercentage { get; set; }

    /// <summary>
    /// Optional email address of the contributor.
    /// </summary>
    public string? Email { get; set; }

    /// <summary>
    /// Optional notes about the contributor.
    /// </summary>
    public string? Notes { get; set; }
}
