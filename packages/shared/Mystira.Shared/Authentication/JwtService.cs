using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
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
/// </summary>
public class JwtService : IJwtService
{
    private readonly JwtOptions _options;
    private readonly ILogger<JwtService> _logger;
    private readonly JwtSecurityTokenHandler _tokenHandler;

    public JwtService(IOptions<JwtOptions> options, ILogger<JwtService> logger)
    {
        _options = options.Value;
        _logger = logger;
        _tokenHandler = new JwtSecurityTokenHandler();
    }

    /// <inheritdoc />
    public async Task<ClaimsPrincipal?> ValidateTokenAsync(string token, CancellationToken cancellationToken = default)
    {
        try
        {
            // For Entra ID tokens, validation is typically handled by the middleware
            // This method provides additional validation if needed
            var tokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = _options.ValidateIssuer,
                ValidateAudience = _options.ValidateAudience,
                ValidateLifetime = _options.ValidateLifetime,
                ValidAudience = _options.Audience,
                ValidIssuer = _options.Authority,
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
}
