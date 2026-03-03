namespace Mystira.Core.Results;

/// <summary>
/// Extension methods for working with Result types.
/// </summary>
public static class ResultExtensions
{
    /// <summary>
    /// Maps a successful result to a new value.
    /// </summary>
    public static Result<TNew> Map<T, TNew>(this Result<T> result, Func<T, TNew> mapper)
    {
        ArgumentNullException.ThrowIfNull(mapper);
        return result.IsSuccess
            ? Result<TNew>.Success(mapper(result.Value))
            : Result<TNew>.Failure(result.Error);
    }

    /// <summary>
    /// Maps a successful result to a new result (flatMap/bind).
    /// </summary>
    public static Result<TNew> Bind<T, TNew>(this Result<T> result, Func<T, Result<TNew>> binder)
    {
        ArgumentNullException.ThrowIfNull(binder);
        return result.IsSuccess
            ? binder(result.Value)
            : Result<TNew>.Failure(result.Error);
    }

    /// <summary>
    /// Maps the error of a failed result to a new error.
    /// </summary>
    public static Result<T> MapError<T>(this Result<T> result, Func<Error, Error> mapper)
    {
        ArgumentNullException.ThrowIfNull(mapper);
        return result.IsFailure
            ? Result<T>.Failure(mapper(result.Error))
            : result;
    }

    /// <summary>
    /// Executes an action if the result is successful.
    /// </summary>
    public static Result<T> Tap<T>(this Result<T> result, Action<T> action)
    {
        ArgumentNullException.ThrowIfNull(action);
        if (result.IsSuccess)
            action(result.Value);
        return result;
    }

    /// <summary>
    /// Executes an action if the result is a failure.
    /// </summary>
    public static Result<T> TapError<T>(this Result<T> result, Action<Error> action)
    {
        ArgumentNullException.ThrowIfNull(action);
        if (result.IsFailure)
            action(result.Error);
        return result;
    }

    /// <summary>
    /// Converts a nullable value to a Result, using the provided error if null.
    /// </summary>
    public static Result<T> ToResult<T>(this T? value, Error errorIfNull) where T : class =>
        value is null ? Result<T>.Failure(errorIfNull) : Result<T>.Success(value);

    /// <summary>
    /// Converts a nullable value to a Result, using the provided error if null.
    /// </summary>
    public static Result<T> ToResult<T>(this T? value, Error errorIfNull) where T : struct =>
        value.HasValue ? Result<T>.Success(value.Value) : Result<T>.Failure(errorIfNull);

    /// <summary>
    /// Combines multiple results into a single result containing all values.
    /// Returns the first error if any result fails.
    /// </summary>
    public static Result<IReadOnlyList<T>> Combine<T>(this IEnumerable<Result<T>> results)
    {
        var list = new List<T>();
        foreach (var result in results)
        {
            if (result.IsFailure)
                return Result<IReadOnlyList<T>>.Failure(result.Error);
            list.Add(result.Value);
        }
        return list;
    }

    /// <summary>
    /// Combines multiple results, collecting all errors if any fail.
    /// </summary>
    public static Result<IReadOnlyList<T>> CombineAll<T>(this IEnumerable<Result<T>> results)
    {
        var values = new List<T>();
        var errors = new List<Error>();

        foreach (var result in results)
        {
            if (result.IsSuccess)
                values.Add(result.Value);
            else
                errors.Add(result.Error);
        }

        if (errors.Count > 0)
        {
            var combinedError = new Error(
                "MULTIPLE_ERRORS",
                $"{errors.Count} error(s) occurred")
            {
                Metadata = new Dictionary<string, object> { ["errors"] = errors }
            };
            return Result<IReadOnlyList<T>>.Failure(combinedError);
        }

        return values;
    }

    /// <summary>
    /// Ensures a condition is met, returning a failure if not.
    /// </summary>
    public static Result<T> Ensure<T>(this Result<T> result, Func<T, bool> predicate, Error error)
    {
        ArgumentNullException.ThrowIfNull(predicate);
        if (result.IsFailure)
            return result;
        return predicate(result.Value) ? result : Result<T>.Failure(error);
    }

    /// <summary>
    /// Converts a Result to a Result with no value.
    /// </summary>
    public static Result ToResult<T>(this Result<T> result) =>
        result.IsSuccess ? Result.Success() : Result.Failure(result.Error);

    /// <summary>
    /// Converts a successful Result to a Result with the specified value.
    /// </summary>
    public static Result<T> ToResult<T>(this Result result, T value) =>
        result.IsSuccess ? Result<T>.Success(value) : Result<T>.Failure(result.Error);

    /// <summary>
    /// Converts a successful Result to a Result with a computed value.
    /// </summary>
    public static Result<T> ToResult<T>(this Result result, Func<T> valueFactory)
    {
        ArgumentNullException.ThrowIfNull(valueFactory);
        return result.IsSuccess ? Result<T>.Success(valueFactory()) : Result<T>.Failure(result.Error);
    }
}
