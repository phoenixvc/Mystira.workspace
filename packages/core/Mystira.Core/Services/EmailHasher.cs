using System.Security.Cryptography;
using System.Text;

namespace Mystira.Core.Services;

/// <summary>
/// Shared email hashing utility for COPPA compliance.
/// Uses SHA-256 with trimmed, lowercased input for consistent hashing.
/// CRITICAL: This must be the single source of truth for email hashing.
/// If this algorithm changes, all stored ParentEmailHash values become invalid.
/// </summary>
public static class EmailHasher
{
    public static string Hash(string email)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(email.Trim().ToLowerInvariant()));
        return Convert.ToHexString(bytes).ToLowerInvariant();
    }
}
