using Mystira.Domain.Entities;
using Mystira.Domain.Enums;

namespace Mystira.Domain.Models;

/// <summary>
/// Represents a user account in the Mystira system.
/// </summary>
public class Account : SoftDeletableEntity
{
    /// <summary>
    /// Gets or sets the email address.
    /// </summary>
    public string Email { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the normalized email for comparison.
    /// </summary>
    public string NormalizedEmail { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets whether the email has been confirmed.
    /// </summary>
    public bool EmailConfirmed { get; set; }

    /// <summary>
    /// Gets or sets the hashed password (null for OAuth-only accounts).
    /// </summary>
    public string? PasswordHash { get; set; }

    /// <summary>
    /// Gets or sets the account status.
    /// </summary>
    public AccountStatus Status { get; set; } = AccountStatus.PendingVerification;

    /// <summary>
    /// Gets or sets the account type.
    /// </summary>
    public AccountType Type { get; set; } = AccountType.Free;

    /// <summary>
    /// Gets or sets the primary authentication provider.
    /// </summary>
    public AuthProvider AuthProvider { get; set; } = AuthProvider.Local;

    /// <summary>
    /// Gets or sets the external provider ID (for OAuth accounts).
    /// </summary>
    public string? ExternalProviderId { get; set; }

    /// <summary>
    /// Gets or sets the external user ID (alias for ExternalProviderId for DTO compatibility).
    /// </summary>
    public string? ExternalUserId
    {
        get => ExternalProviderId;
        set => ExternalProviderId = value;
    }

    /// <summary>
    /// Gets or sets the display name for the account.
    /// </summary>
    public string DisplayName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the list of user profile IDs associated with this account.
    /// </summary>
    public List<string> UserProfileIds { get; set; } = new();

    /// <summary>
    /// Gets or sets the list of completed scenario IDs.
    /// </summary>
    public List<string>? CompletedScenarioIds { get; set; }

    /// <summary>
    /// Gets or sets the subscription details.
    /// </summary>
    public SubscriptionDetails Subscription { get; set; } = new();

    /// <summary>
    /// Gets or sets the account settings.
    /// </summary>
    public AccountSettings? Settings { get; set; }

    /// <summary>
    /// Gets or sets when the account was last logged in.
    /// </summary>
    public DateTime? LastLoginAt { get; set; }

    /// <summary>
    /// Gets or sets the number of failed login attempts.
    /// </summary>
    public int FailedLoginAttempts { get; set; }

    /// <summary>
    /// Gets or sets when the account is locked until (null if not locked).
    /// </summary>
    public DateTime? LockoutEndAt { get; set; }

    /// <summary>
    /// Gets or sets the security stamp for session invalidation.
    /// </summary>
    public string SecurityStamp { get; set; } = Guid.NewGuid().ToString("N");

    /// <summary>
    /// Gets or sets the refresh token for token renewal.
    /// </summary>
    public string? RefreshToken { get; set; }

    /// <summary>
    /// Gets or sets when the refresh token expires.
    /// </summary>
    public DateTime? RefreshTokenExpiresAt { get; set; }

    /// <summary>
    /// Gets or sets whether two-factor authentication is enabled.
    /// </summary>
    public bool TwoFactorEnabled { get; set; }

    /// <summary>
    /// Gets or sets the two-factor secret key.
    /// </summary>
    public string? TwoFactorSecretKey { get; set; }

    /// <summary>
    /// Navigation property to the user profile.
    /// </summary>
    public virtual UserProfile? Profile { get; set; }

    /// <summary>
    /// Checks if the account is currently locked out.
    /// </summary>
    public bool IsLockedOut => LockoutEndAt.HasValue && LockoutEndAt.Value > DateTime.UtcNow;

    /// <summary>
    /// Checks if the account is active and can log in.
    /// </summary>
    public bool CanLogin => Status == AccountStatus.Active && !IsLockedOut && !IsDeleted;

    /// <summary>
    /// Records a failed login attempt.
    /// </summary>
    /// <param name="maxAttempts">Maximum allowed attempts before lockout.</param>
    /// <param name="lockoutDuration">Duration of lockout.</param>
    public void RecordFailedLogin(int maxAttempts = 5, TimeSpan? lockoutDuration = null)
    {
        FailedLoginAttempts++;

        if (FailedLoginAttempts >= maxAttempts)
        {
            LockoutEndAt = DateTime.UtcNow.Add(lockoutDuration ?? TimeSpan.FromMinutes(15));
        }
    }

    /// <summary>
    /// Records a successful login.
    /// </summary>
    public void RecordSuccessfulLogin()
    {
        FailedLoginAttempts = 0;
        LockoutEndAt = null;
        LastLoginAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Regenerates the security stamp, invalidating all existing sessions.
    /// </summary>
    public void RegenerateSecurityStamp()
    {
        SecurityStamp = Guid.NewGuid().ToString("N");
        RefreshToken = null;
        RefreshTokenExpiresAt = null;
    }
}
