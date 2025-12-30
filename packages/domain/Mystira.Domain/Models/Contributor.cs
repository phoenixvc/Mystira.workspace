using Mystira.Domain.Entities;
using Mystira.Domain.Enums;

namespace Mystira.Domain.Models;

/// <summary>
/// Represents a contributor to a scenario or content bundle.
/// </summary>
public class Contributor : Entity
{
    /// <summary>
    /// Gets or sets the scenario or content bundle ID.
    /// </summary>
    public string ContentId { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the content type (scenario, bundle).
    /// </summary>
    public string ContentType { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the user profile ID (if registered user).
    /// </summary>
    public string? UserId { get; set; }

    /// <summary>
    /// Gets or sets the contributor's name.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the contributor's email.
    /// </summary>
    public string? Email { get; set; }

    /// <summary>
    /// Gets or sets the contributor's role.
    /// </summary>
    public ContributorRole Role { get; set; }

    /// <summary>
    /// Gets or sets the contribution percentage (for royalties).
    /// </summary>
    public decimal ContributionPercentage { get; set; }

    /// <summary>
    /// Gets or sets the verification status.
    /// </summary>
    public ContributorVerificationStatus VerificationStatus { get; set; } = ContributorVerificationStatus.Pending;

    /// <summary>
    /// Gets or sets when the contributor was added.
    /// </summary>
    public DateTime AddedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Gets or sets when the contributor was verified.
    /// </summary>
    public DateTime? VerifiedAt { get; set; }

    /// <summary>
    /// Gets or sets additional notes.
    /// </summary>
    public string? Notes { get; set; }

    /// <summary>
    /// Gets or sets the wallet address for payments.
    /// </summary>
    public string? WalletAddress { get; set; }

    /// <summary>
    /// Gets or sets Story Protocol metadata.
    /// </summary>
    public virtual StoryProtocolMetadata? StoryProtocol { get; set; }

    /// <summary>
    /// Navigation to the user profile.
    /// </summary>
    public virtual UserProfile? User { get; set; }
}

/// <summary>
/// Represents Story Protocol blockchain metadata.
/// </summary>
public class StoryProtocolMetadata : Entity
{
    /// <summary>
    /// Gets or sets the contributor ID.
    /// </summary>
    public string ContributorId { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the IP Asset ID on Story Protocol.
    /// </summary>
    public string? IpAssetId { get; set; }

    /// <summary>
    /// Gets or sets the registration transaction hash.
    /// </summary>
    public string? RegistrationTxHash { get; set; }

    /// <summary>
    /// Gets or sets when the registration occurred.
    /// </summary>
    public DateTime? RegisteredAt { get; set; }

    /// <summary>
    /// Gets or sets the royalty module ID.
    /// </summary>
    public string? RoyaltyModuleId { get; set; }

    /// <summary>
    /// Gets or sets the license terms ID.
    /// </summary>
    public string? LicenseTermsId { get; set; }

    /// <summary>
    /// Gets or sets the NFT token ID.
    /// </summary>
    public string? NftTokenId { get; set; }

    /// <summary>
    /// Gets or sets the NFT contract address.
    /// </summary>
    public string? NftContractAddress { get; set; }

    /// <summary>
    /// Gets or sets the chain ID.
    /// </summary>
    public int? ChainId { get; set; }

    /// <summary>
    /// Gets or sets metadata as JSON.
    /// </summary>
    public string? MetadataJson { get; set; }

    /// <summary>
    /// Gets whether this contributor is registered on Story Protocol.
    /// </summary>
    public bool IsRegistered => !string.IsNullOrEmpty(IpAssetId) && RegisteredAt.HasValue;
}

/// <summary>
/// Represents a royalty payment.
/// </summary>
public class RoyaltyPayment : Entity
{
    /// <summary>
    /// Gets or sets the contributor ID.
    /// </summary>
    public string ContributorId { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the content ID.
    /// </summary>
    public string ContentId { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the payment amount in cents.
    /// </summary>
    public long AmountCents { get; set; }

    /// <summary>
    /// Gets or sets the currency.
    /// </summary>
    public string Currency { get; set; } = "USD";

    /// <summary>
    /// Gets or sets the payment status.
    /// </summary>
    public PaymentStatus Status { get; set; } = PaymentStatus.Pending;

    /// <summary>
    /// Gets or sets the payment method.
    /// </summary>
    public PaymentMethod? Method { get; set; }

    /// <summary>
    /// Gets or sets the period start date.
    /// </summary>
    public DateOnly PeriodStart { get; set; }

    /// <summary>
    /// Gets or sets the period end date.
    /// </summary>
    public DateOnly PeriodEnd { get; set; }

    /// <summary>
    /// Gets or sets when the payment was processed.
    /// </summary>
    public DateTime? ProcessedAt { get; set; }

    /// <summary>
    /// Gets or sets the transaction reference.
    /// </summary>
    public string? TransactionReference { get; set; }

    /// <summary>
    /// Gets or sets the transaction hash (for crypto payments).
    /// </summary>
    public string? TransactionHash { get; set; }

    /// <summary>
    /// Gets or sets notes about the payment.
    /// </summary>
    public string? Notes { get; set; }

    /// <summary>
    /// Gets the amount in dollars.
    /// </summary>
    public decimal AmountDollars => AmountCents / 100m;

    /// <summary>
    /// Navigation to the contributor.
    /// </summary>
    public virtual Contributor? Contributor { get; set; }
}
