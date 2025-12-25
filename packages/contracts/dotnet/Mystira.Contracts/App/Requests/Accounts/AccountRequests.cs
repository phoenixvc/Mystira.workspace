namespace Mystira.Contracts.App.Requests.Accounts;

/// <summary>
/// Request to create a new user account.
/// </summary>
public record CreateAccountRequest
{
    /// <summary>
    /// The email address for the account.
    /// </summary>
    public string Email { get; set; } = string.Empty;

    /// <summary>
    /// Optional display name for the account.
    /// </summary>
    public string? DisplayName { get; set; }
}

/// <summary>
/// Request to update an existing user account.
/// </summary>
public record UpdateAccountRequest
{
    /// <summary>
    /// Optional updated display name.
    /// </summary>
    public string? DisplayName { get; set; }

    /// <summary>
    /// Optional updated email address.
    /// </summary>
    public string? Email { get; set; }
}

/// <summary>
/// Request to update subscription information for an account.
/// </summary>
public record UpdateSubscriptionRequest
{
    /// <summary>
    /// The subscription type (e.g., Free, Premium).
    /// </summary>
    public string Type { get; set; } = string.Empty;

    /// <summary>
    /// Optional product identifier from the app store.
    /// </summary>
    public string? ProductId { get; set; }

    /// <summary>
    /// Optional expiration date of the subscription.
    /// </summary>
    public DateTime? ValidUntil { get; set; }

    /// <summary>
    /// Optional purchase token for verification.
    /// </summary>
    public string? PurchaseToken { get; set; }

    /// <summary>
    /// Optional list of individually purchased scenario identifiers.
    /// </summary>
    public List<string>? PurchasedScenarios { get; set; }
}
