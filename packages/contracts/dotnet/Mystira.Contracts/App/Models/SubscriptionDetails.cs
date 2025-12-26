using Mystira.Contracts.App.Enums;

namespace Mystira.Contracts.App.Models;

/// <summary>
/// Represents subscription details for an account.
/// </summary>
public record SubscriptionDetails
{
    /// <summary>
    /// The subscription type.
    /// </summary>
    public SubscriptionType Type { get; set; } = SubscriptionType.Free;

    /// <summary>
    /// App store product identifier.
    /// </summary>
    public string ProductId { get; set; } = string.Empty;

    /// <summary>
    /// When the subscription expires. Null for lifetime or free accounts.
    /// </summary>
    public DateTime? ValidUntil { get; set; }

    /// <summary>
    /// Whether the subscription is currently active.
    /// </summary>
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// Purchase token for app store verification.
    /// </summary>
    public string? PurchaseToken { get; set; }

    /// <summary>
    /// Last time the subscription was verified with the app store.
    /// </summary>
    public DateTime? LastVerified { get; set; }

    /// <summary>
    /// List of individually purchased scenario identifiers.
    /// </summary>
    public List<string> PurchasedScenarios { get; set; } = new();

    /// <summary>
    /// The subscription tier name (e.g., "Free", "Premium").
    /// </summary>
    public string Tier { get; set; } = "Free";

    /// <summary>
    /// When the subscription started.
    /// </summary>
    public DateTime? StartDate { get; set; }

    /// <summary>
    /// When the subscription ends.
    /// </summary>
    public DateTime? EndDate { get; set; }
}
