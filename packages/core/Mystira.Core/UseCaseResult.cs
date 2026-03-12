namespace Mystira.Core;

/// <summary>
/// Represents the result of a use case operation.
/// Allows use cases to return success/failure without throwing exceptions
/// for expected business-rule violations.
/// </summary>
public class UseCaseResult<T>
{
    /// <summary>Whether the operation succeeded.</summary>
    public bool IsSuccess { get; private set; }

    /// <summary>The result data (null on failure).</summary>
    public T? Data { get; private set; }

    /// <summary>The error message (null on success).</summary>
    public string? ErrorMessage { get; private set; }

    /// <summary>Creates a successful result with data.</summary>
    public static UseCaseResult<T> Success(T data)
        => new() { IsSuccess = true, Data = data };

    /// <summary>Creates a failed result with an error message.</summary>
    public static UseCaseResult<T> Failure(string errorMessage)
        => new() { IsSuccess = false, ErrorMessage = errorMessage };
}
