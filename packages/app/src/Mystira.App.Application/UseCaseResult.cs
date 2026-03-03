namespace Mystira.App.Application;

/// <summary>
/// Represents the result of a use case operation.
/// Allows use cases to return success/failure without throwing exceptions
/// for expected business-rule violations.
/// </summary>
public class UseCaseResult<T>
{
    public bool IsSuccess { get; private set; }
    public T? Data { get; private set; }
    public string? ErrorMessage { get; private set; }

    public static UseCaseResult<T> Success(T data)
        => new() { IsSuccess = true, Data = data };

    public static UseCaseResult<T> Failure(string errorMessage)
        => new() { IsSuccess = false, ErrorMessage = errorMessage };
}
