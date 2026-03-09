namespace Mystira.App.Domain.Models;

/// <summary>
/// Parental consent record for COPPA compliance.
/// Tracks consent status between a parent/guardian and a child profile.
/// </summary>
public class ParentalConsent
{
    public string Id { get; set; } = Guid.NewGuid().ToString();

    /// <summary>
    /// SHA-256 hash of the parent's email (never store plain text)
    /// </summary>
    public string ParentEmailHash { get; set; } = string.Empty;

    /// <summary>
    /// The child profile this consent applies to
    /// </summary>
    public string ChildProfileId { get; set; } = string.Empty;

    /// <summary>
    /// Display name of the child (pseudonym, not PII)
    /// </summary>
    public string ChildDisplayName { get; set; } = string.Empty;

    /// <summary>
    /// Current consent status
    /// </summary>
    public ConsentStatus Status { get; set; } = ConsentStatus.Pending;

    /// <summary>
    /// Method used to verify parental identity
    /// </summary>
    public ConsentVerificationMethod VerificationMethod { get; set; } = ConsentVerificationMethod.None;

    /// <summary>
    /// When consent was initially requested
    /// </summary>
    public DateTime RequestedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// When consent was granted (null if still pending)
    /// </summary>
    public DateTime? ConsentedAt { get; set; }

    /// <summary>
    /// When consent was verified via the chosen method (null if not yet verified)
    /// </summary>
    public DateTime? VerifiedAt { get; set; }

    /// <summary>
    /// When consent was revoked (null if not revoked)
    /// </summary>
    public DateTime? RevokedAt { get; set; }

    /// <summary>
    /// Hashed IP address of the consenting parent
    /// </summary>
    public string? IpAddressHash { get; set; }

    /// <summary>
    /// Version of the privacy policy the parent consented to
    /// </summary>
    public string PrivacyPolicyVersion { get; set; } = "1.0";

    /// <summary>
    /// Unique verification token sent to the parent
    /// </summary>
    public string? VerificationToken { get; set; }

    /// <summary>
    /// When the verification token expires
    /// </summary>
    public DateTime? VerificationTokenExpiresAt { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Whether consent has been verified and is currently active
    /// </summary>
    public bool IsActive => Status == ConsentStatus.Verified && RevokedAt == null;

    /// <summary>
    /// Whether the verification token has expired
    /// </summary>
    public bool IsTokenExpired => VerificationTokenExpiresAt.HasValue
        && DateTime.UtcNow > VerificationTokenExpiresAt.Value;

    /// <summary>
    /// Approve the consent request
    /// </summary>
    public void Approve(ConsentVerificationMethod method)
    {
        Status = ConsentStatus.Verified;
        VerificationMethod = method;
        ConsentedAt = DateTime.UtcNow;
        VerifiedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Revoke consent (triggers data deletion workflow)
    /// </summary>
    public void Revoke()
    {
        Status = ConsentStatus.Revoked;
        RevokedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Deny consent request
    /// </summary>
    public void Deny()
    {
        Status = ConsentStatus.Denied;
        UpdatedAt = DateTime.UtcNow;
    }
}

/// <summary>
/// Consent workflow status
/// </summary>
public enum ConsentStatus
{
    Pending,
    EmailSent,
    Verified,
    Denied,
    Revoked,
    Expired
}

/// <summary>
/// Method used to verify parental identity per COPPA requirements
/// </summary>
public enum ConsentVerificationMethod
{
    None,
    Email,
    CreditCard,
    GovernmentId,
    VideoCall,
    SignedForm
}
