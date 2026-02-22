using System.Security.Cryptography;
using System.Text;

namespace Mystira.App.Application;

/// <summary>
/// Deterministic one-way hashing/masking for PII fields in log output.
/// </summary>
public static class PiiMask
{
    /// <summary>
    /// Returns a truncated SHA-256 hex hash of the input (first 12 hex chars).
    /// Deterministic so the same input always produces the same hash for log correlation.
    /// </summary>
    public static string HashId(string? value)
    {
        if (string.IsNullOrEmpty(value))
            return "[empty]";

        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(value));
        return Convert.ToHexString(bytes)[..12].ToLowerInvariant();
    }

    /// <summary>
    /// Returns a deterministic, non-reversible representation of an email address
    /// suitable for log correlation without exposing PII.
    /// e.g., "alice@example.com" -> "[email:a1b2c3d4e5f6]"
    /// </summary>
    public static string MaskEmail(string? email)
    {
        if (string.IsNullOrEmpty(email))
            return "[empty]";

        return $"[email:{HashId(email)}]";
    }
}
