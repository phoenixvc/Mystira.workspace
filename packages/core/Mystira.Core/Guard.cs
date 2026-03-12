using Mystira.Shared.Exceptions;

namespace Mystira.Core;

/// <summary>
/// Argument validation helper to reduce scattered validation boilerplate across handlers.
/// Throws ValidationException for failed preconditions.
/// </summary>
public static class Guard
{
    /// <summary>
    /// Throws if the value is null or empty/whitespace.
    /// </summary>
    public static void AgainstNullOrEmpty(string? value, string parameterName)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ValidationException(parameterName, $"{parameterName} is required");
    }

    /// <summary>
    /// Throws if the value is null.
    /// </summary>
    public static void AgainstNull<T>(T? value, string parameterName) where T : class
    {
        if (value is null)
            throw new ValidationException(parameterName, $"{parameterName} is required");
    }

    /// <summary>
    /// Throws if the collection is null or empty.
    /// </summary>
    public static void AgainstNullOrEmptyCollection<T>(IEnumerable<T>? collection, string parameterName)
    {
        if (collection is null || !collection.Any())
            throw new ValidationException(parameterName, $"{parameterName} must contain at least one item");
    }

    /// <summary>
    /// Throws if the condition is true.
    /// </summary>
    public static void Against(bool condition, string message)
    {
        if (condition)
            throw new ValidationException("input", message);
    }
}
