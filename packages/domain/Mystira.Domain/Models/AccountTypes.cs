namespace Mystira.Domain.Models;

/// <summary>
/// Represents subscription details for an account.
/// </summary>
public class SubscriptionDetails
{
    /// <summary>
    /// Gets or sets the unique identifier for this subscription.
    /// </summary>
    public string Id { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the subscription type.
    /// </summary>
    public SubscriptionType Type { get; set; } = SubscriptionType.Free;

    /// <summary>
    /// Gets or sets the app store product identifier.
    /// </summary>
    public string ProductId { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the expiration date. Null for lifetime or free accounts.
    /// </summary>
    public DateTime? ValidUntil { get; set; }

    /// <summary>
    /// Gets or sets whether the subscription is active.
    /// </summary>
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// Gets or sets the purchase token for app store verification.
    /// </summary>
    public string? PurchaseToken { get; set; }

    /// <summary>
    /// Gets or sets the last time subscription was verified with app store.
    /// </summary>
    public DateTime? LastVerified { get; set; }

    /// <summary>
    /// Gets or sets the list of individual scenario purchases.
    /// </summary>
    public List<string> PurchasedScenarios { get; set; } = new();

    /// <summary>
    /// Gets or sets the subscription tier name.
    /// </summary>
    public string Tier { get; set; } = "Free";

    /// <summary>
    /// Gets or sets the subscription start date.
    /// </summary>
    public DateTime? StartDate { get; set; }

    /// <summary>
    /// Checks if the subscription is currently active.
    /// </summary>
    /// <returns>True if subscription is active and valid.</returns>
    public bool IsSubscriptionActive()
    {
        if (!IsActive)
        {
            return false;
        }

        // Check if subscription has started
        if (StartDate.HasValue && StartDate.Value > DateTime.UtcNow)
        {
            return false;
        }

        // Check if subscription has expired
        if (ValidUntil.HasValue && ValidUntil.Value < DateTime.UtcNow)
        {
            return false;
        }

        return true;
    }
}

/// <summary>
/// Represents account-level settings and preferences.
/// </summary>
public class AccountSettings
{
    /// <summary>
    /// Gets or sets the unique identifier for these settings.
    /// </summary>
    public string Id { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets whether credentials should be cached.
    /// </summary>
    public bool CacheCredentials { get; set; } = true;

    /// <summary>
    /// Gets or sets whether authentication is required on startup.
    /// </summary>
    public bool RequireAuthOnStartup { get; set; } = false;

    /// <summary>
    /// Gets or sets the preferred language code.
    /// </summary>
    public string PreferredLanguage { get; set; } = "en";

    /// <summary>
    /// Gets or sets whether notifications are enabled.
    /// </summary>
    public bool NotificationsEnabled { get; set; } = true;

    /// <summary>
    /// Gets or sets the UI theme.
    /// </summary>
    public string Theme { get; set; } = "Light";
}

/// <summary>
/// Represents the type of subscription.
/// </summary>
public enum SubscriptionType
{
    /// <summary>Limited access tier.</summary>
    Free,
    /// <summary>Monthly subscription tier.</summary>
    Monthly,
    /// <summary>Annual subscription tier.</summary>
    Annual,
    /// <summary>One-time purchase with lifetime access.</summary>
    Lifetime,
    /// <summary>Individual scenario purchases.</summary>
    Individual
}

/// <summary>
/// Represents pricing for a content bundle.
/// </summary>
public class BundlePrice
{
    /// <summary>
    /// Gets or sets the price value.
    /// </summary>
    public decimal Value { get; set; }

    /// <summary>
    /// Gets or sets the currency code.
    /// </summary>
    public string Currency { get; set; } = "USD";
}
