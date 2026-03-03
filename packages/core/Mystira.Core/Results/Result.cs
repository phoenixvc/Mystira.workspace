namespace Mystira.Core.Results;

/// <summary>
/// Represents the result of an operation that can succeed or fail.
/// Use this for operations where failure is an expected outcome, not an exception.
/// </summary>
/// <typeparam name="T">The type of the success value.</typeparam>
public readonly struct Result<T> : IEquatable<Result<T>>
{
    private readonly T? _value;
    private readonly Error? _error;

    /// <summary>
    /// Gets a value indicating whether the result is successful.
    /// </summary>
    public bool IsSuccess { get; }

    /// <summary>
    /// Gets a value indicating whether the result is a failure.
    /// </summary>
    public bool IsFailure => !IsSuccess;

    /// <summary>
    /// Gets the success value. Throws if the result is a failure.
    /// </summary>
    /// <exception cref="InvalidOperationException">Thrown when accessing Value on a failed Result.</exception>
    public T Value => IsSuccess
        ? _value!
        : throw new InvalidOperationException($"Cannot access Value on a failed Result. Error: {_error}");

    /// <summary>
    /// Gets the error. Throws if the result is successful.
    /// </summary>
    /// <exception cref="InvalidOperationException">Thrown when accessing Error on a successful Result.</exception>
    public Error Error => IsFailure
        ? _error!
        : throw new InvalidOperationException("Cannot access Error on a successful Result");

    private Result(T value)
    {
        IsSuccess = true;
        _value = value;
        _error = null;
    }

    private Result(Error error)
    {
        IsSuccess = false;
        _value = default;
        _error = error ?? throw new ArgumentNullException(nameof(error));
    }

    /// <summary>
    /// Creates a successful result with the specified value.
    /// </summary>
    public static Result<T> Success(T value) => new(value);

    /// <summary>
    /// Creates a failed result with the specified error.
    /// </summary>
    public static Result<T> Failure(Error error) => new(error);

    /// <summary>
    /// Implicitly converts a value to a successful result.
    /// </summary>
    public static implicit operator Result<T>(T value) => Success(value);

    /// <summary>
    /// Implicitly converts an error to a failed result.
    /// </summary>
    public static implicit operator Result<T>(Error error) => Failure(error);

    /// <summary>
    /// Pattern matches on the result, executing the appropriate function based on success or failure.
    /// </summary>
    public TResult Match<TResult>(
        Func<T, TResult> onSuccess,
        Func<Error, TResult> onFailure)
    {
        ArgumentNullException.ThrowIfNull(onSuccess);
        ArgumentNullException.ThrowIfNull(onFailure);
        return IsSuccess ? onSuccess(_value!) : onFailure(_error!);
    }

    /// <summary>
    /// Executes an action based on the result state.
    /// </summary>
    public void Switch(
        Action<T> onSuccess,
        Action<Error> onFailure)
    {
        ArgumentNullException.ThrowIfNull(onSuccess);
        ArgumentNullException.ThrowIfNull(onFailure);

        if (IsSuccess)
            onSuccess(_value!);
        else
            onFailure(_error!);
    }

    /// <summary>
    /// Gets the value or a default if the result is a failure.
    /// </summary>
    public T? GetValueOrDefault() => IsSuccess ? _value : default;

    /// <summary>
    /// Gets the value or the specified default if the result is a failure.
    /// </summary>
    public T GetValueOrDefault(T defaultValue) => IsSuccess ? _value! : defaultValue;

    /// <summary>
    /// Gets the value or computes a default if the result is a failure.
    /// </summary>
    public T GetValueOrDefault(Func<Error, T> defaultFactory)
    {
        ArgumentNullException.ThrowIfNull(defaultFactory);
        return IsSuccess ? _value! : defaultFactory(_error!);
    }

    /// <inheritdoc />
    public bool Equals(Result<T> other)
    {
        if (IsSuccess != other.IsSuccess) return false;
        if (IsSuccess) return EqualityComparer<T>.Default.Equals(_value, other._value);
        return _error!.Equals(other._error);
    }

    /// <inheritdoc />
    public override bool Equals(object? obj) => obj is Result<T> other && Equals(other);

    /// <inheritdoc />
    public override int GetHashCode() => IsSuccess
        ? HashCode.Combine(true, _value)
        : HashCode.Combine(false, _error);

    /// <summary>
    /// Determines whether two results are equal.
    /// </summary>
    public static bool operator ==(Result<T> left, Result<T> right) => left.Equals(right);
    
    /// <summary>
    /// Determines whether two results are not equal.
    /// </summary>
    public static bool operator !=(Result<T> left, Result<T> right) => !left.Equals(right);

    /// <inheritdoc />
    public override string ToString() => IsSuccess
        ? $"Success({_value})"
        : $"Failure({_error})";
}

/// <summary>
/// Represents the result of an operation that can succeed (with no value) or fail.
/// </summary>
public readonly struct Result : IEquatable<Result>
{
    private readonly Error? _error;

    /// <summary>
    /// Gets a value indicating whether the result is successful.
    /// </summary>
    public bool IsSuccess { get; }

    /// <summary>
    /// Gets a value indicating whether the result is a failure.
    /// </summary>
    public bool IsFailure => !IsSuccess;

    /// <summary>
    /// Gets the error. Throws if the result is successful.
    /// </summary>
    public Error Error => IsFailure
        ? _error!
        : throw new InvalidOperationException("Cannot access Error on a successful Result");

    private Result(bool isSuccess, Error? error = null)
    {
        IsSuccess = isSuccess;
        _error = error;
    }

    /// <summary>
    /// Creates a successful result.
    /// </summary>
    public static Result Success() => new(true);

    /// <summary>
    /// Creates a failed result with the specified error.
    /// </summary>
    public static Result Failure(Error error) => new(false, error ?? throw new ArgumentNullException(nameof(error)));

    /// <summary>
    /// Creates a successful result with the specified value.
    /// </summary>
    public static Result<T> Success<T>(T value) => Result<T>.Success(value);

    /// <summary>
    /// Creates a failed result with the specified error.
    /// </summary>
    public static Result<T> Failure<T>(Error error) => Result<T>.Failure(error);

    /// <summary>
    /// Implicitly converts an error to a failed result.
    /// </summary>
    public static implicit operator Result(Error error) => Failure(error);

    /// <inheritdoc />
    public bool Equals(Result other) => IsSuccess == other.IsSuccess &&
        (IsSuccess || _error!.Equals(other._error));

    /// <inheritdoc />
    public override bool Equals(object? obj) => obj is Result other && Equals(other);
    
    /// <inheritdoc />
    public override int GetHashCode() => IsSuccess ? HashCode.Combine(true) : HashCode.Combine(false, _error);

    /// <summary>
    /// Determines whether two results are equal.
    /// </summary>
    public static bool operator ==(Result left, Result right) => left.Equals(right);
    
    /// <summary>
    /// Determines whether two results are not equal.
    /// </summary>
    public static bool operator !=(Result left, Result right) => !left.Equals(right);

    /// <inheritdoc />
    public override string ToString() => IsSuccess ? "Success" : $"Failure({_error})";
}
