using System.ComponentModel.DataAnnotations;
using Microsoft.Extensions.Options;

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
    [Required(ErrorMessage = "Authority URL is required")]
    [Url(ErrorMessage = "Authority must be a valid URL")]
    public string Authority { get; set; } = string.Empty;

    /// <summary>
    /// The client/application ID registered in Entra ID
    /// </summary>
    [Required(ErrorMessage = "ClientId is required")]
    public string ClientId { get; set; } = string.Empty;

    /// <summary>
    /// The expected audience for tokens (usually api://{client-id})
    /// </summary>
    [Required(ErrorMessage = "Audience is required")]
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
    [Range(0, 60, ErrorMessage = "ClockSkewMinutes must be between 0 and 60")]
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

/// <summary>
/// Validator for JwtOptions that enforces conditional rules.
/// </summary>
public class JwtOptionsValidator : IValidateOptions<JwtOptions>
{
    public ValidateOptionsResult Validate(string? name, JwtOptions options)
    {
        var failures = new List<string>();

        if (string.IsNullOrWhiteSpace(options.Authority))
        {
            failures.Add("Authority URL is required");
        }
        else if (!Uri.TryCreate(options.Authority, UriKind.Absolute, out var uri) ||
                 (uri.Scheme != Uri.UriSchemeHttps && uri.Scheme != Uri.UriSchemeHttp))
        {
            failures.Add("Authority must be a valid HTTP(S) URL");
        }

        if (string.IsNullOrWhiteSpace(options.ClientId))
        {
            failures.Add("ClientId is required");
        }

        if (string.IsNullOrWhiteSpace(options.Audience))
        {
            failures.Add("Audience is required");
        }

        if (options.ClockSkewMinutes < 0 || options.ClockSkewMinutes > 60)
        {
            failures.Add("ClockSkewMinutes must be between 0 and 60");
        }

        // Conditional validation: SignUpSignInPolicyId required for External ID
        if (options.IsExternalId && string.IsNullOrWhiteSpace(options.SignUpSignInPolicyId))
        {
            failures.Add("SignUpSignInPolicyId is required when IsExternalId is true");
        }

        return failures.Count > 0
            ? ValidateOptionsResult.Fail(failures)
            : ValidateOptionsResult.Success;
    }
}
