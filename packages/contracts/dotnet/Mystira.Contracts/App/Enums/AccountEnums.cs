namespace Mystira.Contracts.App.Enums;

/// <summary>
/// Represents the subscription type for an account.
/// </summary>
public enum SubscriptionType
{
    /// <summary>
    /// Free tier with basic features.
    /// </summary>
    Free = 0,

    /// <summary>
    /// Basic paid subscription.
    /// </summary>
    Basic = 1,

    /// <summary>
    /// Premium subscription with advanced features.
    /// </summary>
    Premium = 2,

    /// <summary>
    /// Family subscription for multiple profiles.
    /// </summary>
    Family = 3,

    /// <summary>
    /// Enterprise subscription for organizations.
    /// </summary>
    Enterprise = 4
}
