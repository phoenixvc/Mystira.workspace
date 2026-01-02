using System.Diagnostics.CodeAnalysis;
using System.Security.Cryptography;

namespace Mystira.Domain.Entities;

/// <summary>
/// Utility class for working with entity IDs.
/// Mystira uses custom sortable string IDs (hex timestamp + random) - never use Guid.Parse on entity IDs.
/// </summary>
public static class EntityId
{
    /// <summary>
    /// Generates a new sortable entity ID.
    /// Format: 12-char uppercase hex timestamp + 14-char uppercase hex random = 26 chars total.
    /// IDs are time-sortable, URL-safe, and avoid Guid.Parse issues.
    /// </summary>
    /// <returns>A new 26-character sortable ID string.</returns>
    public static string NewId()
    {
        // Custom sortable ID format: 12-char hex timestamp + 14-char hex random = 26 chars
        // Timestamp: Unix milliseconds as uppercase hex, zero-padded to 12 chars
        // Random: Cryptographically secure random bytes as uppercase hex
        var timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        Span<byte> randomBytes = stackalloc byte[7];
        RandomNumberGenerator.Fill(randomBytes);
        var randomHex = Convert.ToHexString(randomBytes);
        return $"{timestamp:X12}{randomHex}"[..26].ToUpperInvariant();
    }

    /// <summary>
    /// Validates that a string is a valid entity ID.
    /// </summary>
    /// <param name="id">The ID to validate.</param>
    /// <returns>True if valid.</returns>
    public static bool IsValid([NotNullWhen(true)] string? id)
    {
        if (string.IsNullOrWhiteSpace(id))
            return false;

        // Accept both current format (26 chars) and legacy formats
        // Do NOT validate as Guid - that's the whole point
        return id.Length >= 20 && id.Length <= 40;
    }

    /// <summary>
    /// Validates an ID and throws if invalid.
    /// </summary>
    /// <param name="id">The ID to validate.</param>
    /// <param name="paramName">Parameter name for exception.</param>
    /// <exception cref="ArgumentException">Thrown if ID is invalid.</exception>
    public static void Validate(string? id, string paramName = "id")
    {
        if (!IsValid(id))
        {
            throw new ArgumentException($"Invalid entity ID: '{id}'", paramName);
        }
    }

    /// <summary>
    /// Safely parses an ID without using Guid.Parse.
    /// Returns null if the ID is invalid instead of throwing.
    /// </summary>
    /// <param name="id">The ID string.</param>
    /// <returns>The validated ID or null.</returns>
    public static string? TryParse(string? id)
    {
        return IsValid(id) ? id : null;
    }
}

/// <summary>
/// Extension methods for entity ID validation.
/// </summary>
public static class EntityIdExtensions
{
    /// <summary>
    /// Throws if the ID is not valid.
    /// </summary>
    /// <param name="id">The ID to validate.</param>
    /// <returns>The validated ID.</returns>
    /// <exception cref="ArgumentException">Thrown if ID is invalid.</exception>
    public static string RequireValidId(this string? id)
    {
        EntityId.Validate(id);
        return id!;
    }
}
