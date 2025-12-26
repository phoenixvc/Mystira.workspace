using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;

namespace Mystira.Shared.Authentication;

/// <summary>
/// Options for local JWT token generation and validation.
/// </summary>
public class LocalJwtOptions
{
    /// <summary>
    /// Configuration section name.
    /// </summary>
    public const string SectionName = "JwtSettings";

    /// <summary>
    /// Token issuer (e.g., "Mystira.App").
    /// </summary>
    public string Issuer { get; set; } = string.Empty;

    /// <summary>
    /// Token audience (e.g., "Mystira.App.Api").
    /// </summary>
    public string Audience { get; set; } = string.Empty;

    /// <summary>
    /// Symmetric secret key for HS256 signing. Use RsaPrivateKey for RS256 instead.
    /// </summary>
    public string? SecretKey { get; set; }

    /// <summary>
    /// PEM-encoded RSA private key for RS256 signing (recommended).
    /// </summary>
    public string? RsaPrivateKey { get; set; }

    /// <summary>
    /// PEM-encoded RSA public key for RS256 validation.
    /// </summary>
    public string? RsaPublicKey { get; set; }

    /// <summary>
    /// Access token expiration in hours. Default: 6.
    /// </summary>
    public int AccessTokenExpirationHours { get; set; } = 6;

    /// <summary>
    /// Clock skew tolerance in minutes. Default: 1.
    /// </summary>
    public int ClockSkewMinutes { get; set; } = 1;
}

/// <summary>
/// Service for generating and validating locally-issued JWT tokens.
/// Use this for self-issued tokens (e.g., for your own users).
/// For external IdP token validation (Entra ID, etc.), use JwtService instead.
/// </summary>
public interface ILocalJwtService
{
    /// <summary>
    /// Generates an access token for a user.
    /// </summary>
    string GenerateAccessToken(string userId, string email, string displayName, string? role = null);

    /// <summary>
    /// Generates an access token with custom claims.
    /// </summary>
    string GenerateAccessToken(IEnumerable<Claim> claims);

    /// <summary>
    /// Generates a cryptographically secure refresh token.
    /// </summary>
    string GenerateRefreshToken();

    /// <summary>
    /// Validates a token and returns whether it's valid.
    /// </summary>
    bool ValidateToken(string token);

    /// <summary>
    /// Validates a refresh token against a stored value.
    /// </summary>
    bool ValidateRefreshToken(string token, string storedRefreshToken);

    /// <summary>
    /// Gets the user ID from a token without full validation.
    /// </summary>
    string? GetUserIdFromToken(string token);

    /// <summary>
    /// Validates a token and extracts the user ID.
    /// </summary>
    (bool IsValid, string? UserId) ValidateAndExtractUserId(string token);

    /// <summary>
    /// Extracts user ID from a token without validating its lifetime.
    /// Used for refresh token flow where the access token may be expired.
    /// Still validates signature, issuer, and audience.
    /// </summary>
    (bool IsValid, string? UserId) ExtractUserIdIgnoringExpiry(string token);

    /// <summary>
    /// Validates a token and returns the claims principal.
    /// </summary>
    ClaimsPrincipal? ValidateAndGetPrincipal(string token);
}

/// <summary>
/// Implementation of ILocalJwtService for generating and validating self-issued JWTs.
/// Supports both symmetric (HS256) and asymmetric (RS256) signing.
/// </summary>
public class LocalJwtService : ILocalJwtService
{
    private readonly LocalJwtOptions _options;
    private readonly ILogger<LocalJwtService> _logger;
    private readonly SigningCredentials _signingCredentials;
    private readonly SecurityKey _validationKey;
    private readonly bool _useAsymmetric;

    public LocalJwtService(LocalJwtOptions options, ILogger<LocalJwtService> logger)
    {
        _options = options ?? throw new ArgumentNullException(nameof(options));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        if (string.IsNullOrEmpty(_options.Issuer))
            throw new InvalidOperationException("JWT Issuer not configured.");

        if (string.IsNullOrEmpty(_options.Audience))
            throw new InvalidOperationException("JWT Audience not configured.");

        // Check if asymmetric signing is configured
        _useAsymmetric = !string.IsNullOrEmpty(_options.RsaPrivateKey);

        if (_useAsymmetric)
        {
            try
            {
                var rsa = RSA.Create();
                rsa.ImportFromPem(_options.RsaPrivateKey!);
                var rsaSecurityKey = new RsaSecurityKey(rsa);
                _signingCredentials = new SigningCredentials(rsaSecurityKey, SecurityAlgorithms.RsaSha256);

                // For validation, use public key if available, otherwise use private key
                if (!string.IsNullOrEmpty(_options.RsaPublicKey))
                {
                    var rsaPublic = RSA.Create();
                    rsaPublic.ImportFromPem(_options.RsaPublicKey);
                    _validationKey = new RsaSecurityKey(rsaPublic);
                }
                else
                {
                    _validationKey = rsaSecurityKey;
                }

                _logger.LogInformation("LocalJwtService initialized with RS256 asymmetric signing");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to load RSA private key for asymmetric signing");
                throw new InvalidOperationException("Failed to load RSA private key.", ex);
            }
        }
        else
        {
            if (string.IsNullOrEmpty(_options.SecretKey))
            {
                throw new InvalidOperationException(
                    "JWT signing key not configured. Provide either RsaPrivateKey for RS256 (recommended) or SecretKey for HS256.");
            }

            var key = Encoding.ASCII.GetBytes(_options.SecretKey);
            var symmetricKey = new SymmetricSecurityKey(key);
            _signingCredentials = new SigningCredentials(symmetricKey, SecurityAlgorithms.HmacSha256Signature);
            _validationKey = symmetricKey;

            _logger.LogWarning("LocalJwtService initialized with HS256 symmetric signing. Consider using RS256 for better security.");
        }
    }

    public string GenerateAccessToken(string userId, string email, string displayName, string? role = null)
    {
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, userId),
            new Claim(ClaimTypes.Email, email),
            new Claim(ClaimTypes.Name, displayName),
            new Claim("sub", userId),
            new Claim("email", email),
            new Claim("name", displayName),
            new Claim(ClaimTypes.Role, role ?? "Guest")
        };

        return GenerateAccessToken(claims);
    }

    public string GenerateAccessToken(IEnumerable<Claim> claims)
    {
        var tokenHandler = new JwtSecurityTokenHandler();

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Expires = DateTime.UtcNow.AddHours(_options.AccessTokenExpirationHours),
            Issuer = _options.Issuer,
            Audience = _options.Audience,
            SigningCredentials = _signingCredentials
        };

        var token = tokenHandler.CreateToken(tokenDescriptor);
        return tokenHandler.WriteToken(token);
    }

    public string GenerateRefreshToken()
    {
        var randomNumber = new byte[64];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(randomNumber);
        return Convert.ToBase64String(randomNumber);
    }

    public bool ValidateToken(string token)
    {
        try
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var parameters = GetValidationParameters(validateLifetime: true);
            tokenHandler.ValidateToken(token, parameters, out _);
            return true;
        }
        catch
        {
            return false;
        }
    }

    public bool ValidateRefreshToken(string token, string storedRefreshToken)
    {
        if (string.IsNullOrEmpty(token) || string.IsNullOrEmpty(storedRefreshToken))
            return false;

        return token == storedRefreshToken;
    }

    public string? GetUserIdFromToken(string token)
    {
        try
        {
            var handler = new JwtSecurityTokenHandler();
            var jwtToken = handler.ReadJwtToken(token);
            var userIdClaim = jwtToken.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier || c.Type == "sub");
            return userIdClaim?.Value;
        }
        catch
        {
            return null;
        }
    }

    public (bool IsValid, string? UserId) ValidateAndExtractUserId(string token)
    {
        try
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var parameters = GetValidationParameters(validateLifetime: true);
            var principal = tokenHandler.ValidateToken(token, parameters, out _);
            var userId = principal.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? principal.FindFirst("sub")?.Value;
            return (true, userId);
        }
        catch
        {
            return (false, null);
        }
    }

    public (bool IsValid, string? UserId) ExtractUserIdIgnoringExpiry(string token)
    {
        try
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var parameters = GetValidationParameters(validateLifetime: false);
            var principal = tokenHandler.ValidateToken(token, parameters, out _);
            var userId = principal.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? principal.FindFirst("sub")?.Value;
            return (true, userId);
        }
        catch
        {
            return (false, null);
        }
    }

    public ClaimsPrincipal? ValidateAndGetPrincipal(string token)
    {
        try
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var parameters = GetValidationParameters(validateLifetime: true);
            return tokenHandler.ValidateToken(token, parameters, out _);
        }
        catch
        {
            return null;
        }
    }

    private TokenValidationParameters GetValidationParameters(bool validateLifetime)
    {
        return new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = validateLifetime,
            ValidIssuer = _options.Issuer,
            ValidAudience = _options.Audience,
            IssuerSigningKey = _validationKey,
            ClockSkew = TimeSpan.FromMinutes(_options.ClockSkewMinutes)
        };
    }
}

/// <summary>
/// Extension methods for registering LocalJwtService.
/// </summary>
public static class LocalJwtServiceExtensions
{
    /// <summary>
    /// Adds ILocalJwtService to the service collection.
    /// </summary>
    public static IServiceCollection AddLocalJwtService(this IServiceCollection services, IConfiguration configuration)
    {
        var options = new LocalJwtOptions();
        configuration.GetSection(LocalJwtOptions.SectionName).Bind(options);

        // Also check alternative configuration paths
        if (string.IsNullOrEmpty(options.Issuer))
            options.Issuer = configuration["Jwt:Issuer"] ?? string.Empty;
        if (string.IsNullOrEmpty(options.Audience))
            options.Audience = configuration["Jwt:Audience"] ?? string.Empty;
        if (string.IsNullOrEmpty(options.SecretKey))
            options.SecretKey = configuration["Jwt:Key"];
        if (string.IsNullOrEmpty(options.RsaPrivateKey))
            options.RsaPrivateKey = configuration["Jwt:RsaPrivateKey"];
        if (string.IsNullOrEmpty(options.RsaPublicKey))
            options.RsaPublicKey = configuration["Jwt:RsaPublicKey"];

        services.AddSingleton(options);
        services.AddSingleton<ILocalJwtService, LocalJwtService>();

        return services;
    }

    /// <summary>
    /// Adds ILocalJwtService with explicit options.
    /// </summary>
    public static IServiceCollection AddLocalJwtService(this IServiceCollection services, Action<LocalJwtOptions> configure)
    {
        var options = new LocalJwtOptions();
        configure(options);

        services.AddSingleton(options);
        services.AddSingleton<ILocalJwtService, LocalJwtService>();

        return services;
    }
}
