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

/// <summary>
/// Represents the status of a user account.
/// </summary>
/// <remarks>
/// This type is re-exported from Mystira.Domain.Enums for backward compatibility.
/// New code should use Mystira.Domain.Enums.AccountStatus directly.
/// </remarks>
public enum AccountStatus
{
    /// <inheritdoc cref="DomainAccountStatus.Active"/>
    Active = 0,

    /// <inheritdoc cref="DomainAccountStatus.PendingVerification"/>
    PendingVerification = 1,

    /// <inheritdoc cref="DomainAccountStatus.Suspended"/>
    Suspended = 2,

    /// <inheritdoc cref="DomainAccountStatus.Deactivated"/>
    Deactivated = 3,

    /// <inheritdoc cref="DomainAccountStatus.Deleted"/>
    Deleted = 4
}

/// <summary>
/// Represents the type of user account.
/// </summary>
/// <remarks>
/// This type is re-exported from Mystira.Domain.Enums for backward compatibility.
/// New code should use Mystira.Domain.Enums.AccountType directly.
/// </remarks>
public enum AccountType
{
    /// <inheritdoc cref="DomainAccountType.Free"/>
    Free = 0,

    /// <inheritdoc cref="DomainAccountType.Premium"/>
    Premium = 1,

    /// <inheritdoc cref="DomainAccountType.Educational"/>
    Educational = 2,

    /// <inheritdoc cref="DomainAccountType.Enterprise"/>
    Enterprise = 3
}

/// <summary>
/// Represents the authentication provider used.
/// </summary>
/// <remarks>
/// This type is re-exported from Mystira.Domain.Enums for backward compatibility.
/// New code should use Mystira.Domain.Enums.AuthProvider directly.
/// </remarks>
public enum AuthProvider
{
    /// <inheritdoc cref="DomainAuthProvider.Local"/>
    Local = 0,

    /// <inheritdoc cref="DomainAuthProvider.Google"/>
    Google = 1,

    /// <inheritdoc cref="DomainAuthProvider.Apple"/>
    Apple = 2,

    /// <inheritdoc cref="DomainAuthProvider.Microsoft"/>
    Microsoft = 3,

    /// <inheritdoc cref="DomainAuthProvider.Passwordless"/>
    Passwordless = 4
}

/// <summary>
/// Extension methods for account enum conversions between Contracts and Domain.
/// </summary>
public static class AccountEnumExtensions
{
    /// <summary>
    /// Converts Contracts AccountStatus to Domain AccountStatus.
    /// </summary>
    public static DomainAccountStatus ToDomain(this AccountStatus value)
        => (DomainAccountStatus)(int)value;

    /// <summary>
    /// Converts Domain AccountStatus to Contracts AccountStatus.
    /// </summary>
    public static AccountStatus ToContracts(this DomainAccountStatus value)
        => (AccountStatus)(int)value;

    /// <summary>
    /// Converts Contracts AccountType to Domain AccountType.
    /// </summary>
    public static DomainAccountType ToDomain(this AccountType value)
        => (DomainAccountType)(int)value;

    /// <summary>
    /// Converts Domain AccountType to Contracts AccountType.
    /// </summary>
    public static AccountType ToContracts(this DomainAccountType value)
        => (AccountType)(int)value;

    /// <summary>
    /// Converts Contracts AuthProvider to Domain AuthProvider.
    /// </summary>
    public static DomainAuthProvider ToDomain(this AuthProvider value)
        => (DomainAuthProvider)(int)value;

    /// <summary>
    /// Converts Domain AuthProvider to Contracts AuthProvider.
    /// </summary>
    public static AuthProvider ToContracts(this DomainAuthProvider value)
        => (AuthProvider)(int)value;
}
