namespace Mystira.Shared.Authentication;

/// <summary>
/// Configuration options for JWT authentication.
/// Supports Microsoft Entra ID (Azure AD) and Entra External ID.
/// </summary>
public class JwtOptions
{
    /// <summary>
    /// Configuration section name in appsettings.json
    /// </summary>
    public const string SectionName = "Mystira:Authentication";

    /// <summary>
    /// The authority URL for token validation (e.g., https://login.microsoftonline.com/{tenant-id})
    /// </summary>
    public string Authority { get; set; } = string.Empty;

    /// <summary>
    /// The client/application ID registered in Entra ID
    /// </summary>
    public string ClientId { get; set; } = string.Empty;

    /// <summary>
    /// The expected audience for tokens (usually api://{client-id})
    /// </summary>
    public string Audience { get; set; } = string.Empty;

    /// <summary>
    /// Tenant ID for multi-tenant scenarios
    /// </summary>
    public string? TenantId { get; set; }

    /// <summary>
    /// Whether to validate the token issuer
    /// </summary>
    public bool ValidateIssuer { get; set; } = true;

    /// <summary>
    /// Whether to validate the token audience
    /// </summary>
    public bool ValidateAudience { get; set; } = true;

    /// <summary>
    /// Whether to validate the token lifetime
    /// </summary>
    public bool ValidateLifetime { get; set; } = true;

    /// <summary>
    /// Clock skew tolerance for token validation (in minutes)
    /// </summary>
    public int ClockSkewMinutes { get; set; } = 5;

    /// <summary>
    /// Whether this is an External ID (B2C) configuration
    /// </summary>
    public bool IsExternalId { get; set; } = false;

    /// <summary>
    /// Custom sign-up/sign-in policy for External ID
    /// </summary>
    public string? SignUpSignInPolicyId { get; set; }
}
