using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using Mystira.App.Domain.Models;

namespace Mystira.App.Api.Services;

public class ApiTokenService : IApiTokenService
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<ApiTokenService> _logger;

    public ApiTokenService(IConfiguration configuration, ILogger<ApiTokenService> logger)
    {
        _configuration = configuration;
        _logger = logger;
    }

    public (string AccessToken, DateTime ExpiresAtUtc) CreateAccessToken(Account account, string authProvider)
    {
        var issuer = _configuration["JwtSettings:Issuer"];
        var audience = _configuration["JwtSettings:Audience"];

        if (string.IsNullOrWhiteSpace(issuer) || string.IsNullOrWhiteSpace(audience))
        {
            throw new InvalidOperationException("JWT issuer/audience is not configured");
        }

        var signingCredentials = BuildSigningCredentials();
        var expiresAtUtc = DateTime.UtcNow.AddHours(8);

        var claims = new List<Claim>
        {
            new("account_id", account.Id),
            new(JwtRegisteredClaimNames.Sub, account.Id),
            new(JwtRegisteredClaimNames.Email, account.Email),
            new(JwtRegisteredClaimNames.UniqueName, account.DisplayName),
            new(ClaimTypes.Name, account.DisplayName),
            new(ClaimTypes.Email, account.Email),
            new(ClaimTypes.Role, account.Role),
            new("auth_provider", authProvider)
        };

        var token = new JwtSecurityToken(
            issuer: issuer,
            audience: audience,
            claims: claims,
            notBefore: DateTime.UtcNow,
            expires: expiresAtUtc,
            signingCredentials: signingCredentials);

        var tokenValue = new JwtSecurityTokenHandler().WriteToken(token);
        return (tokenValue, expiresAtUtc);
    }

    private SigningCredentials BuildSigningCredentials()
    {
        var rsaPrivateKey = _configuration["JwtSettings:RsaPrivateKey"];
        if (!string.IsNullOrWhiteSpace(rsaPrivateKey))
        {
            using var rsa = RSA.Create();
            rsa.ImportFromPem(rsaPrivateKey);
            var key = new RsaSecurityKey(rsa.ExportParameters(true));
            return new SigningCredentials(key, SecurityAlgorithms.RsaSha256);
        }

        var secret = _configuration["JwtSettings:SecretKey"];
        if (!string.IsNullOrWhiteSpace(secret))
        {
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret));
            _logger.LogWarning("Using symmetric JWT signing for API tokens");
            return new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        }

        throw new InvalidOperationException("No JWT signing key configured for token issuance");
    }
}
