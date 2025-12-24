using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Protocols;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Microsoft.IdentityModel.Tokens;

namespace Mystira.Shared.Authentication;

/// <summary>
/// Service for JWT token operations including validation and claims extraction.
/// </summary>
public interface IJwtService
{
    /// <summary>
    /// Validates a JWT token and returns the claims principal.
    /// </summary>
    Task<ClaimsPrincipal?> ValidateTokenAsync(string token, CancellationToken cancellationToken = default);

    /// <summary>
    /// Extracts claims from a token without full validation (for debugging/logging).
    /// </summary>
    IEnumerable<Claim> ExtractClaims(string token);

    /// <summary>
    /// Gets the user ID from claims.
    /// </summary>
    string? GetUserId(ClaimsPrincipal principal);

    /// <summary>
    /// Gets the user's roles from claims.
    /// </summary>
    IEnumerable<string> GetRoles(ClaimsPrincipal principal);
}

/// <summary>
/// Implementation of JWT service for Microsoft Entra ID tokens.
/// Uses OIDC discovery to fetch signing keys from the authority's metadata endpoint.
/// </summary>
public class JwtService : IJwtService
{
    private readonly JwtOptions _options;
    private readonly ILogger<JwtService> _logger;
    private readonly JwtSecurityTokenHandler _tokenHandler;
    private readonly ConfigurationManager<OpenIdConnectConfiguration> _configurationManager;

    /// <summary>
    /// Initializes a new instance of the <see cref="JwtService"/> class.
    /// </summary>
    /// <param name="options">JWT authentication options.</param>
    /// <param name="logger">Logger instance.</param>
    public JwtService(IOptions<JwtOptions> options, ILogger<JwtService> logger)
    {
        _options = options.Value;
        _logger = logger;
        _tokenHandler = new JwtSecurityTokenHandler();

        // Set up OIDC configuration manager to fetch signing keys from metadata endpoint
        var metadataAddress = GetMetadataAddress(_options);
        _configurationManager = new ConfigurationManager<OpenIdConnectConfiguration>(
            metadataAddress,
            new OpenIdConnectConfigurationRetriever(),
            new HttpDocumentRetriever());
    }

    /// <inheritdoc />
    public async Task<ClaimsPrincipal?> ValidateTokenAsync(string token, CancellationToken cancellationToken = default)
    {
        try
        {
            // Fetch the OIDC configuration (includes signing keys)
            var config = await _configurationManager.GetConfigurationAsync(cancellationToken);

            var tokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = _options.ValidateIssuer,
                ValidateAudience = _options.ValidateAudience,
                ValidateLifetime = _options.ValidateLifetime,
                ValidateIssuerSigningKey = true,
                ValidAudience = _options.Audience,
                // For Entra ID v2, issuer is typically https://login.microsoftonline.com/{tenant}/v2.0
                // Use the issuer from OIDC config for accurate validation
                ValidIssuer = config.Issuer,
                ValidIssuers = GetValidIssuers(_options, config),
                IssuerSigningKeys = config.SigningKeys,
                ClockSkew = TimeSpan.FromMinutes(_options.ClockSkewMinutes),
            };

            var principal = await Task.Run(() =>
                _tokenHandler.ValidateToken(token, tokenValidationParameters, out _),
                cancellationToken);

            return principal;
        }
        catch (SecurityTokenException ex)
        {
            _logger.LogWarning(ex, "Token validation failed");
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error during token validation");
            return null;
        }
    }

    /// <inheritdoc />
    public IEnumerable<Claim> ExtractClaims(string token)
    {
        try
        {
            var jwtToken = _tokenHandler.ReadJwtToken(token);
            return jwtToken.Claims;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to extract claims from token");
            return Enumerable.Empty<Claim>();
        }
    }

    /// <inheritdoc />
    public string? GetUserId(ClaimsPrincipal principal)
    {
        // Try common claim types for user ID
        return principal.FindFirst(ClaimTypes.NameIdentifier)?.Value
            ?? principal.FindFirst("oid")?.Value  // Entra ID object ID
            ?? principal.FindFirst("sub")?.Value; // Standard subject claim
    }

    /// <inheritdoc />
    public IEnumerable<string> GetRoles(ClaimsPrincipal principal)
    {
        return principal.FindAll(ClaimTypes.Role)
            .Concat(principal.FindAll("roles")) // Entra ID app roles
            .Select(c => c.Value)
            .Distinct();
    }

    private static string GetMetadataAddress(JwtOptions options)
    {
        var authority = options.Authority.TrimEnd('/');

        // For External ID (B2C), use the policy-specific metadata endpoint
        if (options.IsExternalId && !string.IsNullOrEmpty(options.SignUpSignInPolicyId))
        {
            return $"{authority}/{options.SignUpSignInPolicyId}/v2.0/.well-known/openid-configuration";
        }

        // For standard Entra ID
        return $"{authority}/v2.0/.well-known/openid-configuration";
    }

    private static IEnumerable<string> GetValidIssuers(JwtOptions options, OpenIdConnectConfiguration config)
    {
        var issuers = new List<string>();

        // Add issuer from OIDC config
        if (!string.IsNullOrEmpty(config.Issuer))
        {
            issuers.Add(config.Issuer);
        }

        // Add common Entra ID issuer formats if tenant ID is provided
        if (!string.IsNullOrEmpty(options.TenantId))
        {
            issuers.Add($"https://login.microsoftonline.com/{options.TenantId}/v2.0");
            issuers.Add($"https://sts.windows.net/{options.TenantId}/");
        }

        return issuers.Distinct();
    }
}
