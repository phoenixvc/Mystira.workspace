using Mystira.Domain.Entities;
using Mystira.Domain.Enums;

namespace Mystira.Domain.Models;

/// <summary>
/// Represents a pending user signup that has not yet been completed.
/// Used to track users who have started the registration process but have not
/// verified their email or completed all required steps.
/// </summary>
public class PendingSignup : SoftDeletableEntity
{
    /// <summary>
    /// Gets or sets the email address for the signup.
    /// </summary>
    public string Email { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the normalized email for comparison.
    /// </summary>
    public string NormalizedEmail { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the hashed password.
    /// </summary>
    public string? PasswordHash { get; set; }

    /// <summary>
    /// Gets or sets the display name provided during signup.
    /// </summary>
    public string? DisplayName { get; set; }

    /// <summary>
    /// Gets or sets the authentication provider used for signup.
    /// </summary>
    public AuthProvider AuthProvider { get; set; } = AuthProvider.Local;

    /// <summary>
    /// Gets or sets the external provider ID (for OAuth signups).
    /// </summary>
    public string? ExternalProviderId { get; set; }

    /// <summary>
    /// Gets or sets the verification token for email confirmation.
    /// </summary>
    public string VerificationToken { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets when the verification token expires.
    /// </summary>
    public DateTime VerificationTokenExpiresAt { get; set; }

    /// <summary>
    /// Gets or sets the status of the pending signup.
    /// </summary>
    public PendingSignupStatus Status { get; set; } = PendingSignupStatus.Pending;

    /// <summary>
    /// Gets or sets the number of verification emails sent.
    /// </summary>
    public int VerificationEmailsSent { get; set; }

    /// <summary>
    /// Gets or sets when the last verification email was sent.
    /// </summary>
    public DateTime? LastVerificationEmailSentAt { get; set; }

    /// <summary>
    /// Gets or sets the IP address used during signup.
    /// </summary>
    public string? IpAddress { get; set; }

    /// <summary>
    /// Gets or sets the user agent string from the signup request.
    /// </summary>
    public string? UserAgent { get; set; }

    /// <summary>
    /// Gets or sets the referral code used during signup.
    /// </summary>
    public string? ReferralCode { get; set; }

    /// <summary>
    /// Gets or sets additional metadata as JSON.
    /// </summary>
    public string? MetadataJson { get; set; }

    /// <summary>
    /// Gets or sets when the signup was started.
    /// </summary>
    public DateTime StartedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Gets or sets when the signup was completed (email verified and account created).
    /// </summary>
    public DateTime? CompletedAt { get; set; }

    /// <summary>
    /// Gets or sets the account ID created from this signup.
    /// </summary>
    public string? AccountId { get; set; }

    /// <summary>
    /// Checks if the verification token has expired.
    /// </summary>
    public bool IsTokenExpired => DateTime.UtcNow > VerificationTokenExpiresAt;

    /// <summary>
    /// Checks if this signup can be completed.
    /// </summary>
    public bool CanComplete => Status == PendingSignupStatus.Pending && !IsTokenExpired && !IsDeleted;

    /// <summary>
    /// Generates a new verification token.
    /// </summary>
    /// <param name="expirationHours">Hours until the token expires.</param>
    public void GenerateVerificationToken(int expirationHours = 24)
    {
        VerificationToken = Guid.NewGuid().ToString("N") + Guid.NewGuid().ToString("N");
        VerificationTokenExpiresAt = DateTime.UtcNow.AddHours(expirationHours);
    }

    /// <summary>
    /// Records that a verification email was sent.
    /// </summary>
    public void RecordVerificationEmailSent()
    {
        VerificationEmailsSent++;
        LastVerificationEmailSentAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Marks the signup as completed with the created account ID.
    /// </summary>
    /// <param name="accountId">The ID of the created account.</param>
    public void Complete(string accountId)
    {
        Status = PendingSignupStatus.Completed;
        CompletedAt = DateTime.UtcNow;
        AccountId = accountId;
    }

    /// <summary>
    /// Marks the signup as expired.
    /// </summary>
    public void Expire()
    {
        Status = PendingSignupStatus.Expired;
    }

    /// <summary>
    /// Marks the signup as cancelled.
    /// </summary>
    public void Cancel()
    {
        Status = PendingSignupStatus.Cancelled;
    }
}

/// <summary>
/// Status of a pending signup.
/// </summary>
public enum PendingSignupStatus
{
    /// <summary>
    /// The signup is pending verification.
    /// </summary>
    Pending = 0,

    /// <summary>
    /// The signup has been completed and an account was created.
    /// </summary>
    Completed = 1,

    /// <summary>
    /// The signup has expired.
    /// </summary>
    Expired = 2,

    /// <summary>
    /// The signup was cancelled by the user.
    /// </summary>
    Cancelled = 3
}
