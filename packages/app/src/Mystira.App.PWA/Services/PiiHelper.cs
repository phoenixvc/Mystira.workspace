using System.Security.Cryptography;
using System.Text;

namespace Mystira.App.PWA.Services;

/// <summary>
/// Provides one-way hashing utilities for anonymizing PII in client-side logs.
/// </summary>
public static class PiiHelper
{
    /// <summary>
    /// Returns a stable 8-character hex hash of the given identifier.
    /// Suitable for log correlation without exposing raw identifiers.
    /// </summary>
    public static string AnonymizeId(string? id)
    {
        if (string.IsNullOrEmpty(id))
            return "[empty]";

        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(id));
        return Convert.ToHexString(bytes, 0, 4).ToLowerInvariant();
    }
}
