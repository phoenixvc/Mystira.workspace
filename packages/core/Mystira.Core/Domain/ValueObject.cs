namespace Mystira.Core.Domain;

/// <summary>
/// Base class for value objects. Value objects are compared by their values, not identity.
/// </summary>
public abstract class ValueObject : IEquatable<ValueObject>
{
    /// <summary>
    /// Gets the atomic values that make up this value object for equality comparison.
    /// </summary>
    protected abstract IEnumerable<object?> GetEqualityComponents();

    /// <inheritdoc />
    public bool Equals(ValueObject? other)
    {
        if (other is null) return false;
        if (ReferenceEquals(this, other)) return true;
        if (GetType() != other.GetType()) return false;

        return GetEqualityComponents().SequenceEqual(other.GetEqualityComponents());
    }

    /// <inheritdoc />
    public override bool Equals(object? obj) => Equals(obj as ValueObject);

    /// <inheritdoc />
    public override int GetHashCode()
    {
        return GetEqualityComponents()
            .Aggregate(0, (hash, component) =>
                HashCode.Combine(hash, component?.GetHashCode() ?? 0));
    }

    /// <summary>
    /// Determines whether two value objects are equal.
    /// </summary>
    public static bool operator ==(ValueObject? left, ValueObject? right) =>
        left is null ? right is null : left.Equals(right);

    /// <summary>
    /// Determines whether two value objects are not equal.
    /// </summary>
    public static bool operator !=(ValueObject? left, ValueObject? right) => !(left == right);
}

/// <summary>
/// Base class for single-value value objects.
/// </summary>
/// <typeparam name="T">The type of the wrapped value.</typeparam>
public abstract class SingleValueObject<T> : ValueObject
    where T : notnull
{
    /// <summary>
    /// Gets the wrapped value.
    /// </summary>
    public T Value { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="SingleValueObject{T}"/> class.
    /// </summary>
    /// <param name="value">The value to wrap.</param>
    protected SingleValueObject(T value)
    {
        Value = value;
    }

    /// <inheritdoc />
    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Value;
    }

    /// <inheritdoc />
    public override string ToString() => Value.ToString() ?? string.Empty;

    /// <summary>
    /// Implicitly converts a <see cref="SingleValueObject{T}"/> to its underlying value.
    /// </summary>
    public static implicit operator T(SingleValueObject<T> valueObject) => valueObject.Value;
}
