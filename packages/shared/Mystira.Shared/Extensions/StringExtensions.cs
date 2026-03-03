using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;

namespace Mystira.Shared.Extensions;

/// <summary>
/// Extension methods for string manipulation.
/// Provides common string utilities usable across all services.
/// </summary>
public static partial class StringExtensions
{
    /// <summary>
    /// Converts a string to title case (first letter of each word capitalized).
    /// </summary>
    /// <param name="value">The string to convert.</param>
    /// <returns>The string in title case, or the original value if null or empty.</returns>
    /// <example>
    /// "hello world".ToTitleCase() returns "Hello World"
    /// "THE QUICK BROWN FOX".ToTitleCase() returns "The Quick Brown Fox"
    /// </example>
    public static string ToTitleCase(this string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return value ?? string.Empty;
        }

        var textInfo = CultureInfo.CurrentCulture.TextInfo;
        return textInfo.ToTitleCase(value.ToLower(CultureInfo.CurrentCulture));
    }

    /// <summary>
    /// Converts a string to title case using a specific culture.
    /// </summary>
    /// <param name="value">The string to convert.</param>
    /// <param name="culture">The culture to use for casing rules.</param>
    /// <returns>The string in title case, or the original value if null or empty.</returns>
    public static string ToTitleCase(this string? value, CultureInfo culture)
    {
        ArgumentNullException.ThrowIfNull(culture);

        if (string.IsNullOrWhiteSpace(value))
        {
            return value ?? string.Empty;
        }

        return culture.TextInfo.ToTitleCase(value.ToLower(culture));
    }

    /// <summary>
    /// Truncates a string to the specified maximum length.
    /// </summary>
    /// <param name="value">The string to truncate.</param>
    /// <param name="maxLength">The maximum length of the resulting string.</param>
    /// <param name="suffix">Optional suffix to append when truncated (e.g., "...").</param>
    /// <returns>The truncated string, or the original value if shorter than maxLength.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when maxLength is negative.</exception>
    /// <example>
    /// "Hello, World!".Truncate(5) returns "Hello"
    /// "Hello, World!".Truncate(8, "...") returns "Hello..."
    /// </example>
    public static string Truncate(this string? value, int maxLength, string? suffix = null)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(maxLength);

        if (string.IsNullOrEmpty(value))
        {
            return value ?? string.Empty;
        }

        suffix ??= string.Empty;

        if (value.Length <= maxLength)
        {
            return value;
        }

        // If suffix is provided, we need to account for its length
        if (suffix.Length > 0)
        {
            var truncateLength = maxLength - suffix.Length;
            if (truncateLength <= 0)
            {
                return suffix.Length <= maxLength ? suffix[..maxLength] : suffix[..maxLength];
            }

            return string.Concat(value.AsSpan(0, truncateLength), suffix);
        }

        return value[..maxLength];
    }

    /// <summary>
    /// Converts a string to snake_case.
    /// </summary>
    /// <param name="value">The string to convert.</param>
    /// <returns>The string in snake_case format.</returns>
    /// <example>
    /// "HelloWorld".ToSnakeCase() returns "hello_world"
    /// "XMLParser".ToSnakeCase() returns "xml_parser"
    /// "getHTTPResponse".ToSnakeCase() returns "get_http_response"
    /// </example>
    public static string ToSnakeCase(this string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return value ?? string.Empty;
        }

        // Handle consecutive uppercase letters (acronyms) and regular casing
        var result = SnakeCaseRegex().Replace(value, "$1_$2");
        return result.ToLowerInvariant();
    }

    /// <summary>
    /// Converts a string to kebab-case.
    /// </summary>
    /// <param name="value">The string to convert.</param>
    /// <returns>The string in kebab-case format.</returns>
    /// <example>
    /// "HelloWorld".ToKebabCase() returns "hello-world"
    /// "XMLParser".ToKebabCase() returns "xml-parser"
    /// </example>
    public static string ToKebabCase(this string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return value ?? string.Empty;
        }

        // Reuse snake_case logic then replace underscores with hyphens
        return value.ToSnakeCase().Replace('_', '-');
    }

    /// <summary>
    /// Converts a string to camelCase.
    /// </summary>
    /// <param name="value">The string to convert.</param>
    /// <returns>The string in camelCase format.</returns>
    /// <example>
    /// "HelloWorld".ToCamelCase() returns "helloWorld"
    /// "hello_world".ToCamelCase() returns "helloWorld"
    /// </example>
    public static string ToCamelCase(this string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return value ?? string.Empty;
        }

        var pascalCase = ToPascalCase(value);
        if (pascalCase.Length == 0)
        {
            return pascalCase;
        }

        return char.ToLowerInvariant(pascalCase[0]) + pascalCase[1..];
    }

    /// <summary>
    /// Converts a string to PascalCase.
    /// </summary>
    /// <param name="value">The string to convert.</param>
    /// <returns>The string in PascalCase format.</returns>
    /// <example>
    /// "hello_world".ToPascalCase() returns "HelloWorld"
    /// "hello-world".ToPascalCase() returns "HelloWorld"
    /// </example>
    public static string ToPascalCase(this string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return value ?? string.Empty;
        }

        var words = WordSplitRegex().Split(value);
        var result = new StringBuilder();

        foreach (var word in words)
        {
            if (string.IsNullOrEmpty(word))
            {
                continue;
            }

            result.Append(char.ToUpperInvariant(word[0]));
            if (word.Length > 1)
            {
                result.Append(word[1..].ToLowerInvariant());
            }
        }

        return result.ToString();
    }

    // Regex for snake_case conversion - handles "XMLParser" -> "xml_parser" and "HelloWorld" -> "hello_world"
    [GeneratedRegex(@"([a-z0-9])([A-Z])|([A-Z]+)([A-Z][a-z])")]
    private static partial Regex SnakeCaseRegex();

    // Regex for splitting words on underscores, hyphens, spaces, and case boundaries
    [GeneratedRegex(@"[\s_\-]+|(?<=[a-z])(?=[A-Z])")]
    private static partial Regex WordSplitRegex();
}
