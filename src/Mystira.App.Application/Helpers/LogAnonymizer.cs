using System.Security.Cryptography;
using System.Text;

namespace Mystira.App.Application.Helpers;

/// <summary>
/// Provides one-way hashing utilities for anonymizing PII in log output.
/// Produces stable, truncated SHA-256 hashes suitable for log correlation
/// without exposing raw identifiers or email addresses.
/// </summary>
public static class LogAnonymizer
{
    /// <summary>
    /// Returns a stable 8-character hex hash of the given identifier.
    /// </summary>
    public static string HashId(string? id)
    {
        if (string.IsNullOrEmpty(id))
            return "[empty]";

        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(id));
        return Convert.ToHexString(bytes, 0, 4).ToLowerInvariant();
    }

    /// <summary>
    /// Returns a stable 8-character hex hash of the given email address.
    /// </summary>
    public static string HashEmail(string? email)
    {
        if (string.IsNullOrEmpty(email))
            return "[empty]";

        var normalized = email.Trim().ToLowerInvariant();
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(normalized));
        return Convert.ToHexString(bytes, 0, 4).ToLowerInvariant();
    }

    /// <summary>
    /// Sanitizes a user-provided string for safe inclusion in log entries.
    /// Removes control characters (newlines, tabs, etc.) to prevent log forging/injection.
    /// </summary>
    public static string SanitizeForLog(string? value, int maxLength = 200)
    {
        if (string.IsNullOrEmpty(value))
            return "[empty]";

        var sanitized = new StringBuilder(Math.Min(value.Length, maxLength));
        foreach (var c in value)
        {
            if (sanitized.Length >= maxLength)
                break;

            sanitized.Append(char.IsControl(c) ? '_' : c);
        }

        return sanitized.ToString();
    }
}
