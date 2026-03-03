// Re-export domain enums for backward compatibility
global using DomainAccountStatus = Mystira.Domain.Enums.AccountStatus;
global using DomainAccountType = Mystira.Domain.Enums.AccountType;
global using DomainAuthProvider = Mystira.Domain.Enums.AuthProvider;

namespace Mystira.Contracts.App.Enums;

/// <summary>
/// Represents the subscription type for an account.
/// </summary>
public enum SubscriptionType
{
    /// <summary>Free tier with basic features.</summary>
    Free = 0,
    /// <summary>Basic paid subscription.</summary>
    Basic = 1,
    /// <summary>Premium subscription with advanced features.</summary>
    Premium = 2,
    /// <summary>Family subscription for multiple profiles.</summary>
    Family = 3,
    /// <summary>Enterprise subscription for organizations.</summary>
    Enterprise = 4
}

/// <summary>
/// Represents the status of a user account.
/// </summary>
/// <remarks>
/// This type mirrors Mystira.Domain.Enums.AccountStatus for backward compatibility.
/// </remarks>
public enum AccountStatus
{
    /// <summary>The account is active and in good standing.</summary>
    Active = 0,
    /// <summary>The account is pending email verification.</summary>
    PendingVerification = 1,
    /// <summary>The account has been suspended.</summary>
    Suspended = 2,
    /// <summary>The account has been deactivated by the user.</summary>
    Deactivated = 3,
    /// <summary>The account has been deleted.</summary>
    Deleted = 4
}

/// <summary>
/// Represents the type of user account.
/// </summary>
/// <remarks>
/// This type mirrors Mystira.Domain.Enums.AccountType for backward compatibility.
/// </remarks>
public enum AccountType
{
    /// <summary>A free tier account.</summary>
    Free = 0,
    /// <summary>A premium subscription account.</summary>
    Premium = 1,
    /// <summary>An educational institution account.</summary>
    Educational = 2,
    /// <summary>An enterprise/business account.</summary>
    Enterprise = 3
}

/// <summary>
/// Represents the authentication provider used.
/// </summary>
/// <remarks>
/// This type mirrors Mystira.Domain.Enums.AuthProvider for backward compatibility.
/// </remarks>
public enum AuthProvider
{
    /// <summary>Local email/password authentication.</summary>
    Local = 0,
    /// <summary>Google OAuth.</summary>
    Google = 1,
    /// <summary>Apple Sign In.</summary>
    Apple = 2,
    /// <summary>Microsoft/Azure AD.</summary>
    Microsoft = 3,
    /// <summary>Passwordless magic link.</summary>
    Passwordless = 4
}

/// <summary>
/// Extension methods for account enum conversions between Contracts and Domain.
/// </summary>
public static class AccountEnumExtensions
{
    /// <summary>Converts to Domain AccountStatus.</summary>
    public static DomainAccountStatus ToDomain(this AccountStatus value) => (DomainAccountStatus)(int)value;
    /// <summary>Converts to Contracts AccountStatus.</summary>
    public static AccountStatus ToContracts(this DomainAccountStatus value) => (AccountStatus)(int)value;

    /// <summary>Converts to Domain AccountType.</summary>
    public static DomainAccountType ToDomain(this AccountType value) => (DomainAccountType)(int)value;
    /// <summary>Converts to Contracts AccountType.</summary>
    public static AccountType ToContracts(this DomainAccountType value) => (AccountType)(int)value;

    /// <summary>Converts to Domain AuthProvider.</summary>
    public static DomainAuthProvider ToDomain(this AuthProvider value) => (DomainAuthProvider)(int)value;
    /// <summary>Converts to Contracts AuthProvider.</summary>
    public static AuthProvider ToContracts(this DomainAuthProvider value) => (AuthProvider)(int)value;
}
