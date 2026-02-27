using System.Security.Cryptography;
using System.Text;

namespace Mystira.App.Application.Services;

/// <summary>
/// Utility service for generating and hashing magic-link tokens.
/// </summary>
public static class MagicLinkTokenService
{
    public static string GenerateRawToken()
    {
        return Convert.ToBase64String(RandomNumberGenerator.GetBytes(32));
    }

    public static string HashToken(string rawToken)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(rawToken));
        return Convert.ToHexString(bytes);
    }
}
