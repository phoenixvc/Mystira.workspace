using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using Mystira.Domain.Models;
using Mystira.Domain.Enums;
using Mystira.Domain.ValueObjects;

namespace Mystira.Identity.Api.Services;

public class IdentityTokenService : IIdentityTokenService
{
    private readonly IConfiguration _configuration;

    public IdentityTokenService(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public (string AccessToken, DateTime ExpiresAtUtc) CreateAccountToken(Account account, string authProvider)
    {
        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, account.Id),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new(ClaimTypes.Name, account.DisplayName),
            new(ClaimTypes.Email, account.Email),
            new(ClaimTypes.Role, account.Role ?? string.Empty),
            new("account_id", account.Id),
            new("auth_provider", authProvider)
        };

        return CreateToken(claims);
    }

    public (string AccessToken, DateTime ExpiresAtUtc) CreateAdminToken(string username, IReadOnlyCollection<string> roles)
    {
        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, $"admin:{username}"),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new(ClaimTypes.Name, username),
            new("auth_provider", "admin_local")
        };

        foreach (var role in roles)
        {
            claims.Add(new Claim(ClaimTypes.Role, role));
            claims.Add(new Claim("role", role));
        }

        return CreateToken(claims);
    }

    private (string AccessToken, DateTime ExpiresAtUtc) CreateToken(IEnumerable<Claim> claims)
    {
        var issuer = _configuration["JwtSettings:Issuer"] ?? "mystira-identity-api";
        var audience = _configuration["JwtSettings:Audience"] ?? "mystira-platform";
        var jwtRsaPrivateKey = _configuration["JwtSettings:RsaPrivateKey"];
        var jwtKey = _configuration["JwtSettings:SecretKey"];

        if (string.IsNullOrWhiteSpace(issuer) || string.IsNullOrWhiteSpace(audience))
        {
            throw new InvalidOperationException("JWT issuer and audience must be configured.");
        }

        var key = GetSigningKey(jwtRsaPrivateKey, jwtKey);
        var algorithm = !string.IsNullOrWhiteSpace(jwtRsaPrivateKey)
            ? SecurityAlgorithms.RsaSha256
            : SecurityAlgorithms.HmacSha256;

        var signingCredentials = new SigningCredentials(key, algorithm);
        var expiresAtUtc = DateTime.UtcNow.AddHours(8);

        var token = new JwtSecurityToken(
            issuer: issuer,
            audience: audience,
            claims: claims,
            expires: expiresAtUtc,
            signingCredentials: signingCredentials);

        var encoded = new JwtSecurityTokenHandler().WriteToken(token);
        return (encoded, expiresAtUtc);
    }

    private static SecurityKey GetSigningKey(string? jwtRsaPrivateKey, string? jwtKey)
    {
        if (!string.IsNullOrWhiteSpace(jwtRsaPrivateKey))
        {
            using var rsa = RSA.Create();
            rsa.ImportFromPem(jwtRsaPrivateKey);
            return new RsaSecurityKey(rsa.ExportParameters(true));
        }

        if (!string.IsNullOrWhiteSpace(jwtKey))
        {
            return new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey));
        }

        throw new InvalidOperationException("JWT signing key is not configured.");
    }
}
