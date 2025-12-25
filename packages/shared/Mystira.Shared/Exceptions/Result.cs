namespace Mystira.Shared.Exceptions;

/// <summary>
/// Represents the result of an operation that may succeed or fail.
/// Provides a functional alternative to throwing exceptions.
/// </summary>
/// <typeparam name="T">The type of the success value.</typeparam>
public readonly struct Result<T>
{
    private readonly T? _value;
    private readonly Error? _error;

    /// <summary>
    /// Whether the operation succeeded.
    /// </summary>
    public bool IsSuccess { get; }

    /// <summary>
    /// Whether the operation failed.
    /// </summary>
    public bool IsFailure => !IsSuccess;

    /// <summary>
    /// The success value. Throws if the result is a failure.
    /// </summary>
    public T Value => IsSuccess
        ? _value!
        : throw new InvalidOperationException($"Cannot access Value on a failed result. Error: {_error}");

    /// <summary>
    /// The error. Throws if the result is a success.
    /// </summary>
    public Error Error => IsFailure
        ? _error!
        : throw new InvalidOperationException("Cannot access Error on a successful result.");

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
        _error = error;
    }

    /// <summary>
    /// Creates a successful result.
    /// </summary>
    public static Result<T> Success(T value) => new(value);

    /// <summary>
    /// Creates a failed result.
    /// </summary>
    public static Result<T> Failure(Error error) => new(error);

    /// <summary>
    /// Creates a failed result from an error code and message.
    /// </summary>
    public static Result<T> Failure(string code, string message) =>
        new(new Error(code, message));

    /// <summary>
    /// Creates a failed result from an exception.
    /// </summary>
    public static Result<T> Failure(Exception exception) =>
        new(Error.FromException(exception));

    /// <summary>
    /// Implicit conversion from value to success result.
    /// </summary>
    public static implicit operator Result<T>(T value) => Success(value);

    /// <summary>
    /// Implicit conversion from error to failure result.
    /// </summary>
    public static implicit operator Result<T>(Error error) => Failure(error);

    /// <summary>
    /// Maps the success value to a new type.
    /// </summary>
    public Result<TNew> Map<TNew>(Func<T, TNew> mapper)
    {
        return IsSuccess
            ? Result<TNew>.Success(mapper(_value!))
            : Result<TNew>.Failure(_error!);
    }

    /// <summary>
    /// Chains another operation that returns a Result.
    /// </summary>
    public Result<TNew> Bind<TNew>(Func<T, Result<TNew>> binder)
    {
        return IsSuccess ? binder(_value!) : Result<TNew>.Failure(_error!);
    }

    /// <summary>
    /// Executes one of two functions based on success/failure.
    /// </summary>
    public TResult Match<TResult>(
        Func<T, TResult> onSuccess,
        Func<Error, TResult> onFailure)
    {
        return IsSuccess ? onSuccess(_value!) : onFailure(_error!);
    }

    /// <summary>
    /// Gets the value or a default if failed.
    /// </summary>
    public T GetValueOrDefault(T defaultValue = default!)
    {
        return IsSuccess ? _value! : defaultValue;
    }

    /// <summary>
    /// Gets the value or throws the error as an exception.
    /// </summary>
    public T GetValueOrThrow()
    {
        if (IsFailure)
        {
            throw new MystiraException(_error!.Code, _error.Message);
        }
        return _value!;
    }
}

/// <summary>
/// Represents a unit result (success with no value or failure).
/// </summary>
public readonly struct Result
{
    private readonly Error? _error;

    /// <summary>
    /// Whether the result represents a success.
    /// </summary>
    public bool IsSuccess { get; }

    /// <summary>
    /// Whether the result represents a failure.
    /// </summary>
    public bool IsFailure => !IsSuccess;

    /// <summary>
    /// The error if the result is a failure.
    /// </summary>
    /// <exception cref="InvalidOperationException">Thrown when accessed on a successful result.</exception>
    public Error Error => IsFailure
        ? _error!
        : throw new InvalidOperationException("Cannot access Error on a successful result.");

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
    /// <param name="error">The error.</param>
    public static Result Failure(Error error) => new(false, error);

    /// <summary>
    /// Creates a failed result with the specified error code and message.
    /// </summary>
    /// <param name="code">The error code.</param>
    /// <param name="message">The error message.</param>
    public static Result Failure(string code, string message) =>
        new(false, new Error(code, message));

    /// <summary>
    /// Implicitly converts an error to a failed result.
    /// </summary>
    /// <param name="error">The error.</param>
    public static implicit operator Result(Error error) => Failure(error);

    /// <summary>
    /// Pattern matches the result to produce a value.
    /// </summary>
    /// <typeparam name="TResult">The result type.</typeparam>
    /// <param name="onSuccess">Function to call on success.</param>
    /// <param name="onFailure">Function to call on failure.</param>
    public TResult Match<TResult>(
        Func<TResult> onSuccess,
        Func<Error, TResult> onFailure)
    {
        return IsSuccess ? onSuccess() : onFailure(_error!);
    }
}

/// <summary>
/// Represents an error with a code and message.
/// </summary>
public record Error(string Code, string Message)
{
    /// <summary>
    /// Creates an error from an exception.
    /// </summary>
    public static Error FromException(Exception ex) =>
        new(ex.GetType().Name.ToUpperInvariant(), ex.Message);

    /// <summary>
    /// Common validation error.
    /// </summary>
    public static Error Validation(string message) =>
        new("VALIDATION_ERROR", message);

    /// <summary>
    /// Common not found error.
    /// </summary>
    public static Error NotFound(string resourceType, string? id = null) =>
        new("NOT_FOUND", id != null
            ? $"{resourceType} with ID '{id}' was not found."
            : $"{resourceType} was not found.");

    /// <summary>
    /// Common unauthorized error.
    /// </summary>
    public static Error Unauthorized(string? message = null) =>
        new("UNAUTHORIZED", message ?? "Authentication is required.");

    /// <summary>
    /// Common forbidden error.
    /// </summary>
    public static Error Forbidden(string? message = null) =>
        new("FORBIDDEN", message ?? "You do not have permission to perform this action.");

    /// <summary>
    /// Common conflict error.
    /// </summary>
    public static Error Conflict(string message) =>
        new("CONFLICT", message);

    /// <summary>
    /// Common internal error.
    /// </summary>
    public static Error Internal(string? message = null) =>
        new("INTERNAL_ERROR", message ?? "An unexpected error occurred.");
}
