namespace Mystira.Domain.Enums;

/// <summary>
/// Represents the status of a user account.
/// </summary>
public enum AccountStatus
{
    /// <summary>
    /// The account is active and in good standing.
    /// </summary>
    Active = 0,

    /// <summary>
    /// The account is pending email verification.
    /// </summary>
    PendingVerification = 1,

    /// <summary>
    /// The account has been suspended.
    /// </summary>
    Suspended = 2,

    /// <summary>
    /// The account has been deactivated by the user.
    /// </summary>
    Deactivated = 3,

    /// <summary>
    /// The account has been deleted.
    /// </summary>
    Deleted = 4
}

/// <summary>
/// Represents the type of user account.
/// </summary>
public enum AccountType
{
    /// <summary>
    /// A free tier account.
    /// </summary>
    Free = 0,

    /// <summary>
    /// A premium subscription account.
    /// </summary>
    Premium = 1,

    /// <summary>
    /// An educational institution account.
    /// </summary>
    Educational = 2,

    /// <summary>
    /// An enterprise/business account.
    /// </summary>
    Enterprise = 3
}

/// <summary>
/// Represents the authentication provider used.
/// </summary>
public enum AuthProvider
{
    /// <summary>
    /// Local email/password authentication.
    /// </summary>
    Local = 0,

    /// <summary>
    /// Google OAuth.
    /// </summary>
    Google = 1,

    /// <summary>
    /// Apple Sign In.
    /// </summary>
    Apple = 2,

    /// <summary>
    /// Microsoft/Azure AD.
    /// </summary>
    Microsoft = 3,

    /// <summary>
    /// Passwordless magic link.
    /// </summary>
    Passwordless = 4
}
