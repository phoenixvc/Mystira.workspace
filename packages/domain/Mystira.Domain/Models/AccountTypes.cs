namespace Mystira.Domain.Models;

/// <summary>
/// Represents subscription details for an account.
/// </summary>
public class SubscriptionDetails
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public SubscriptionType Type { get; set; } = SubscriptionType.Free;
    public string ProductId { get; set; } = string.Empty; // App store product identifier
    public DateTime? ValidUntil { get; set; } // null for lifetime or free accounts
    public bool IsActive { get; set; } = true;
    public string? PurchaseToken { get; set; } // For app store verification
    public DateTime? LastVerified { get; set; } // Last time subscription was verified with app store
    public List<string> PurchasedScenarios { get; set; } = new(); // Individual scenario purchases
    public string Tier { get; set; } = "Free";
    public DateTime? StartDate { get; set; } = DateTime.UtcNow;
    public DateTime? EndDate { get; set; } = DateTime.MaxValue;

    public bool IsSubscriptionActive()
    {
        if (!IsActive)
        {
            return false;
        }

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
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public bool CacheCredentials { get; set; } = true;
    public bool RequireAuthOnStartup { get; set; } = false;
    public string PreferredLanguage { get; set; } = "en";
    public bool NotificationsEnabled { get; set; } = true;
    public string? Theme { get; set; } = "Light";
}

/// <summary>
/// Represents the type of subscription.
/// </summary>
public enum SubscriptionType
{
    Free,           // Limited access
    Monthly,        // Monthly subscription
    Annual,         // Annual subscription
    Lifetime,       // One-time purchase with lifetime updates
    Individual      // Individual scenario purchases
}

/// <summary>
/// Represents pricing for a content bundle.
/// </summary>
public class BundlePrice
{
    public decimal Value { get; set; }
    public string Currency { get; set; } = "USD";
}
