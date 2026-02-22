namespace Mystira.App.Application;

/// <summary>
/// Argument validation helper to reduce scattered validation boilerplate across handlers.
/// Throws ArgumentException for failed preconditions.
/// </summary>
public static class Guard
{
    /// <summary>
    /// Throws if the value is null or empty/whitespace.
    /// </summary>
    public static void AgainstNullOrEmpty(string? value, string parameterName)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException($"{parameterName} is required", parameterName);
    }

    /// <summary>
    /// Throws if the value is null.
    /// </summary>
    public static void AgainstNull<T>(T? value, string parameterName) where T : class
    {
        if (value is null)
            throw new ArgumentNullException(parameterName, $"{parameterName} is required");
    }

    /// <summary>
    /// Throws if the collection is null or empty.
    /// </summary>
    public static void AgainstNullOrEmptyCollection<T>(IEnumerable<T>? collection, string parameterName)
    {
        if (collection is null || !collection.Any())
            throw new ArgumentException($"{parameterName} must contain at least one item", parameterName);
    }

    /// <summary>
    /// Throws if the condition is true.
    /// </summary>
    public static void Against(bool condition, string message)
    {
        if (condition)
            throw new ArgumentException(message);
    }
}
