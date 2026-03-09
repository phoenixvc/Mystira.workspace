using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace Mystira.DevHub.CLI.Services;

public class AuthService : IAuthService
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<AuthService> _logger;
    private readonly TokenValidationParameters _tokenValidationParameters;

    public AuthService(IConfiguration configuration, ILogger<AuthService> logger)
    {
        _configuration = configuration;
        _logger = logger;

        // Configure JWT validation parameters
        var jwtIssuer = _configuration["JwtSettings:Issuer"] ?? "mystira-identity-api";
        var jwtAudience = _configuration["JwtSettings:Audience"] ?? "mystira-platform";
        var jwtRsaPublicKey = _configuration["JwtSettings:RsaPublicKey"];
        var jwtKey = _configuration["JwtSettings:SecretKey"];

        if (string.IsNullOrWhiteSpace(jwtRsaPublicKey) && string.IsNullOrWhiteSpace(jwtKey))
        {
            // For development, create a default key
            jwtKey = $"DevHubDevKey-{Guid.NewGuid():N}";
            _logger.LogWarning("JWT signing key not configured, using development key");
        }

        _tokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtIssuer,
            ValidAudience = jwtAudience,
            ClockSkew = TimeSpan.FromMinutes(5)
        };

        if (!string.IsNullOrWhiteSpace(jwtRsaPublicKey))
        {
            using var rsa = System.Security.Cryptography.RSA.Create();
            rsa.ImportFromPem(jwtRsaPublicKey);
            _tokenValidationParameters.IssuerSigningKey = new RsaSecurityKey(rsa.ExportParameters(false));
        }
        else if (!string.IsNullOrWhiteSpace(jwtKey))
        {
            _tokenValidationParameters.IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey));
        }
        else
        {
            throw new InvalidOperationException("JWT signing key not configured");
        }
    }

    /// <summary>
    /// Validates a JWT token using the configured validation parameters.
    /// </summary>
    /// <param name="token">The JWT token to validate.</param>
    /// <returns>True if the token is valid; false otherwise.</returns>
    public async Task<bool> ValidateTokenAsync(string token)
    {
        try
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var result = await tokenHandler.ValidateTokenAsync(token, _tokenValidationParameters);
            return result.IsValid;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Token validation failed");
            return false;
        }
    }

    /// <summary>
    /// Extracts the claims principal from a valid JWT token.
    /// </summary>
    /// <param name="token">The JWT token to extract claims from.</param>
    /// <returns>The claims principal if valid; null otherwise.</returns>
    public Task<ClaimsPrincipal?> GetPrincipalAsync(string token)
    {
        try
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var principal = tokenHandler.ValidateToken(token, _tokenValidationParameters, out var validatedToken) as ClaimsPrincipal;
            return Task.FromResult<ClaimsPrincipal?>(principal);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to get principal from token");
            return Task.FromResult<ClaimsPrincipal?>(null);
        }
    }

    public async Task<bool> IsAuthorizedAsync(string token, string requiredRole = "admin")
    {
        try
        {
            var principal = await GetPrincipalAsync(token);
            if (principal == null)
            {
                return false;
            }

            // Check if user has the required role
            var roleClaim = principal.FindFirst(ClaimTypes.Role) ?? principal.FindFirst("role");
            if (roleClaim == null)
            {
                _logger.LogWarning("Token does not contain role claim");
                return false;
            }

            var userRoles = roleClaim.Value.Split(',', StringSplitOptions.RemoveEmptyEntries);
            var hasRequiredRole = userRoles.Contains(requiredRole, StringComparer.OrdinalIgnoreCase);

            if (!hasRequiredRole)
            {
                _logger.LogWarning("User {Roles} does not have required role {RequiredRole}",
                    string.Join(",", userRoles), requiredRole);
                return false;
            }

            // Log successful authorization for audit
            var userId = principal.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "unknown";
            var userName = principal.FindFirst(ClaimTypes.Name)?.Value ?? "unknown";
            _logger.LogInformation("User {UserId} ({UserName}) authorized for DevHub operations", userId, userName);

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Authorization check failed");
            return false;
        }
    }
}
