namespace Mystira.Shared.Messaging.Events;

/// <summary>
/// Published when a content bundle is created.
/// </summary>
public sealed record BundleCreated : IntegrationEventBase
{
    /// <summary>
    /// The bundle ID.
    /// </summary>
    public required string BundleId { get; init; }

    /// <summary>
    /// Bundle name.
    /// </summary>
    public required string Name { get; init; }

    /// <summary>
    /// Creator's account ID.
    /// </summary>
    public required string CreatorId { get; init; }

    /// <summary>
    /// Number of items in bundle.
    /// </summary>
    public required int ItemCount { get; init; }

    /// <summary>
    /// Bundle type (scenario_pack, cosmetic_bundle, season_pass).
    /// </summary>
    public required string BundleType { get; init; }

    /// <summary>
    /// Price in cents (0 if free).
    /// </summary>
    public int PriceCents { get; init; }
}

/// <summary>
/// Published when a bundle is purchased.
/// </summary>
public sealed record BundlePurchased : IntegrationEventBase
{
    /// <summary>
    /// The user's account ID.
    /// </summary>
    public required string AccountId { get; init; }

    /// <summary>
    /// The bundle ID.
    /// </summary>
    public required string BundleId { get; init; }

    /// <summary>
    /// Payment ID.
    /// </summary>
    public required string PaymentId { get; init; }

    /// <summary>
    /// Amount paid in cents.
    /// </summary>
    public required int AmountCents { get; init; }

    /// <summary>
    /// Currency code.
    /// </summary>
    public required string Currency { get; init; }

    /// <summary>
    /// Discount applied if any.
    /// </summary>
    public int DiscountPercent { get; init; }
}

/// <summary>
/// Published when a bundle is activated/redeemed.
/// </summary>
public sealed record BundleActivated : IntegrationEventBase
{
    /// <summary>
    /// The user's account ID.
    /// </summary>
    public required string AccountId { get; init; }

    /// <summary>
    /// The bundle ID.
    /// </summary>
    public required string BundleId { get; init; }

    /// <summary>
    /// How it was obtained (purchase, gift, reward, code).
    /// </summary>
    public required string AcquisitionMethod { get; init; }

    /// <summary>
    /// Number of items unlocked.
    /// </summary>
    public required int ItemsUnlocked { get; init; }
}

/// <summary>
/// Published when a bundle is gifted to another user.
/// </summary>
public sealed record BundleGifted : IntegrationEventBase
{
    /// <summary>
    /// The gifter's account ID.
    /// </summary>
    public required string GifterAccountId { get; init; }

    /// <summary>
    /// The recipient's account ID.
    /// </summary>
    public required string RecipientAccountId { get; init; }

    /// <summary>
    /// The bundle ID.
    /// </summary>
    public required string BundleId { get; init; }

    /// <summary>
    /// Payment ID.
    /// </summary>
    public required string PaymentId { get; init; }

    /// <summary>
    /// Gift message if any.
    /// </summary>
    public string? Message { get; init; }
}

/// <summary>
/// Published when a promo code is redeemed.
/// </summary>
public sealed record PromoCodeRedeemed : IntegrationEventBase
{
    /// <summary>
    /// The user's account ID.
    /// </summary>
    public required string AccountId { get; init; }

    /// <summary>
    /// The promo code that was redeemed.
    /// </summary>
    public required string PromoCode { get; init; }

    /// <summary>
    /// The type of discount applied (e.g., percentage, fixed_amount, bundle_unlock).
    /// </summary>
    public required string DiscountType { get; init; }

    /// <summary>
    /// The numeric value of the discount (e.g., 20 for 20% off, 5.99 for $5.99 off).
    /// </summary>
    public required decimal DiscountValue { get; init; }

    /// <summary>
    /// Optional identifier of what the discount was applied to (e.g., bundle ID, subscription tier).
    /// </summary>
    public string? AppliedTo { get; init; }
}
