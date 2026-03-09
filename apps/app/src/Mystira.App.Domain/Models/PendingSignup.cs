namespace Mystira.App.Domain.Models;

/// <summary>
/// Represents a pending email-based signup and magic-link authentication flow.
/// </summary>
public class PendingSignup
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string Email { get; set; } = string.Empty;
    public string? DisplayName { get; set; }
    public string? VerificationTokenHash { get; set; }
    public DateTime? VerificationTokenExpiresAt { get; set; }
    public DateTime? VerifiedAt { get; set; }
    public DateTime? VerifiedUntil { get; set; }
    public DateTime? ConsumedAt { get; set; }
    public PendingSignupStatus Status { get; set; } = PendingSignupStatus.Pending;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    public bool IsVerificationTokenExpired =>
        VerificationTokenExpiresAt.HasValue && DateTime.UtcNow > VerificationTokenExpiresAt.Value;

    public bool IsVerifiedWindowExpired =>
        VerifiedUntil.HasValue && DateTime.UtcNow > VerifiedUntil.Value;

    public void SetToken(string tokenHash, DateTime expiresAtUtc)
    {
        VerificationTokenHash = tokenHash;
        VerificationTokenExpiresAt = expiresAtUtc;
        Status = PendingSignupStatus.EmailSent;
        UpdatedAt = DateTime.UtcNow;
    }

    public void MarkVerified(DateTime verifiedUntilUtc)
    {
        VerifiedAt = DateTime.UtcNow;
        VerifiedUntil = verifiedUntilUtc;
        Status = PendingSignupStatus.Verified;
        UpdatedAt = DateTime.UtcNow;
    }

    public void MarkConsumed()
    {
        ConsumedAt = DateTime.UtcNow;
        Status = PendingSignupStatus.Consumed;
        VerificationTokenHash = null;
        VerificationTokenExpiresAt = null;
        UpdatedAt = DateTime.UtcNow;
    }

    public void MarkExpired()
    {
        Status = PendingSignupStatus.Expired;
        UpdatedAt = DateTime.UtcNow;
    }
}

public enum PendingSignupStatus
{
    Pending,
    EmailSent,
    Verified,
    Consumed,
    Expired
}
