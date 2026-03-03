using System.Reflection;

namespace Mystira.Domain.Primitives;

/// <summary>
/// Base class for string-based enumeration types.
/// Provides type-safe, extensible string enums with predefined values.
/// </summary>
/// <typeparam name="T">The derived enum type.</typeparam>
public abstract class StringEnum<T> : IEquatable<StringEnum<T>>, IComparable<StringEnum<T>>
    where T : StringEnum<T>
{
    private static readonly Lazy<Dictionary<string, T>> _all = new(GetAllValues, LazyThreadSafetyMode.ExecutionAndPublication);
    private static readonly Lazy<IReadOnlyList<T>> _list = new(() => _all.Value.Values.ToList().AsReadOnly());

    /// <summary>
    /// Gets the string value of this enum.
    /// </summary>
    public string Value { get; }

    /// <summary>
    /// Gets the display name for this enum value.
    /// </summary>
    public virtual string DisplayName => Value;

    /// <summary>
    /// Initializes a new instance of the string enum.
    /// </summary>
    /// <param name="value">The string value.</param>
    protected StringEnum(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException("Value cannot be null or whitespace.", nameof(value));

        Value = value;
    }

    /// <summary>
    /// Gets all predefined values for this enum type.
    /// </summary>
    public static IReadOnlyList<T> All => _list.Value;

    /// <summary>
    /// Gets all predefined values as a dictionary keyed by value.
    /// </summary>
    public static IReadOnlyDictionary<string, T> AllByValue => _all.Value;

    /// <summary>
    /// Parses a string value to the corresponding enum value.
    /// </summary>
    /// <param name="value">The string value to parse.</param>
    /// <returns>The parsed enum value.</returns>
    /// <exception cref="ArgumentException">Thrown if the value is not valid.</exception>
    public static T Parse(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException("Value cannot be null or whitespace.", nameof(value));

        if (_all.Value.TryGetValue(value, out var result))
            return result;

        throw new ArgumentException($"'{value}' is not a valid {typeof(T).Name}.", nameof(value));
    }

    /// <summary>
    /// Tries to parse a string value to the corresponding enum value.
    /// </summary>
    /// <param name="value">The string value to parse.</param>
    /// <param name="result">The parsed enum value if successful.</param>
    /// <returns>True if parsing was successful.</returns>
    public static bool TryParse(string? value, out T? result)
    {
        result = null;

        if (string.IsNullOrWhiteSpace(value))
            return false;

        return _all.Value.TryGetValue(value, out result);
    }

    /// <summary>
    /// Gets an enum value by its string value, or null if not found.
    /// </summary>
    /// <param name="value">The string value.</param>
    /// <returns>The enum value or null.</returns>
    public static T? FromValue(string? value)
    {
        TryParse(value, out var result);
        return result;
    }

    /// <summary>
    /// Checks if a value is defined in this enum type.
    /// </summary>
    /// <param name="value">The value to check.</param>
    /// <returns>True if the value is defined.</returns>
    public static bool IsDefined(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return false;

        return _all.Value.ContainsKey(value);
    }

    private static Dictionary<string, T> GetAllValues()
    {
        var type = typeof(T);
        var fields = type.GetFields(BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly);

        var values = new Dictionary<string, T>(StringComparer.OrdinalIgnoreCase);

        foreach (var field in fields)
        {
            if (field.FieldType == type && field.GetValue(null) is T value)
            {
                values[value.Value] = value;
            }
        }

        // Also check static properties
        var properties = type.GetProperties(BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly);
        foreach (var prop in properties)
        {
            if (prop.PropertyType == type && prop.GetValue(null) is T value && !values.ContainsKey(value.Value))
            {
                values[value.Value] = value;
            }
        }

        return values;
    }

    /// <inheritdoc />
    public bool Equals(StringEnum<T>? other)
    {
        if (other is null)
            return false;

        return string.Equals(Value, other.Value, StringComparison.OrdinalIgnoreCase);
    }

    /// <inheritdoc />
    public override bool Equals(object? obj)
    {
        return obj is StringEnum<T> other && Equals(other);
    }

    /// <inheritdoc />
    public override int GetHashCode()
    {
        return StringComparer.OrdinalIgnoreCase.GetHashCode(Value);
    }

    /// <inheritdoc />
    public int CompareTo(StringEnum<T>? other)
    {
        if (other is null)
            return 1;

        return string.Compare(Value, other.Value, StringComparison.OrdinalIgnoreCase);
    }

    /// <inheritdoc />
    public override string ToString() => Value;

    /// <summary>
    /// Implicit conversion to string.
    /// </summary>
    public static implicit operator string(StringEnum<T> value) => value.Value;

    /// <summary>
    /// Equality operator.
    /// </summary>
    public static bool operator ==(StringEnum<T>? left, StringEnum<T>? right)
    {
        if (left is null)
            return right is null;

        return left.Equals(right);
    }

    /// <summary>
    /// Inequality operator.
    /// </summary>
    public static bool operator !=(StringEnum<T>? left, StringEnum<T>? right)
    {
        return !(left == right);
    }

    /// <summary>
    /// Less than operator.
    /// </summary>
    public static bool operator <(StringEnum<T>? left, StringEnum<T>? right)
    {
        return left is null ? right is not null : left.CompareTo(right) < 0;
    }

    /// <summary>
    /// Less than or equal operator.
    /// </summary>
    public static bool operator <=(StringEnum<T>? left, StringEnum<T>? right)
    {
        return left is null || left.CompareTo(right) <= 0;
    }

    /// <summary>
    /// Greater than operator.
    /// </summary>
    public static bool operator >(StringEnum<T>? left, StringEnum<T>? right)
    {
        return left is not null && left.CompareTo(right) > 0;
    }

    /// <summary>
    /// Greater than or equal operator.
    /// </summary>
    public static bool operator >=(StringEnum<T>? left, StringEnum<T>? right)
    {
        return left is null ? right is null : left.CompareTo(right) >= 0;
    }
}
