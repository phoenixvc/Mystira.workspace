using System.Diagnostics.CodeAnalysis;

namespace Mystira.Shared.Data;

/// <summary>
/// Utility class for working with entity IDs.
/// Mystira uses string IDs (ULID format) - never use Guid.Parse on entity IDs.
/// </summary>
public static class EntityId
{
    /// <summary>
    /// Generates a new ULID-format entity ID.
    /// ULIDs are sortable, URL-safe, and avoid Guid.Parse issues.
    /// </summary>
    /// <returns>A new ULID string.</returns>
    public static string NewId()
    {
        // ULID format: 01ARZ3NDEKTSV4RRFFQ69G5FAV (26 chars, Crockford Base32)
        // For now, use a timestamp + random approach that's sortable
        var timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        var random = Random.Shared.NextInt64();
        return $"{timestamp:X12}{random:X8}"[..26].ToUpperInvariant();
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

        // Accept both ULID format (26 chars) and legacy formats
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
