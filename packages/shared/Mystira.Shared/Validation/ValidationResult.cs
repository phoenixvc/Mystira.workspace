namespace Mystira.Shared.Validation;

/// <summary>
/// Standardized validation result that maps to RFC 7807 Problem Details.
/// </summary>
public sealed record ValidationResult
{
    /// <summary>
    /// Gets whether the validation passed (no errors).
    /// </summary>
    public bool IsValid => Errors.Count == 0;

    /// <summary>
    /// Gets the list of validation errors.
    /// </summary>
    public IReadOnlyList<ValidationError> Errors { get; init; } = [];

    /// <summary>
    /// Creates a successful validation result with no errors.
    /// </summary>
    public static ValidationResult Success() => new();

    /// <summary>
    /// Creates a failed validation result with the specified errors.
    /// </summary>
    public static ValidationResult Failure(IEnumerable<ValidationError> errors)
        => new() { Errors = errors.ToList() };

    /// <summary>
    /// Creates a failed validation result with a single error.
    /// </summary>
    public static ValidationResult Failure(string propertyName, string errorMessage)
        => new() { Errors = [new ValidationError(propertyName, errorMessage)] };

    /// <summary>
    /// Creates a failed validation result from FluentValidation failures.
    /// </summary>
    public static ValidationResult FromFluentValidation(
        IEnumerable<FluentValidation.Results.ValidationFailure> failures)
    {
        var errors = failures
            .Select(f => new ValidationError(
                f.PropertyName,
                f.ErrorMessage,
                f.ErrorCode,
                f.AttemptedValue))
            .ToList();

        return new ValidationResult { Errors = errors };
    }

    /// <summary>
    /// Converts this result to a dictionary format suitable for API responses.
    /// Groups errors by property name.
    /// </summary>
    public IDictionary<string, string[]> ToDictionary()
    {
        return Errors
            .GroupBy(e => e.PropertyName)
            .ToDictionary(
                g => g.Key,
                g => g.Select(e => e.ErrorMessage).ToArray());
    }
}

/// <summary>
/// Represents a single validation error with detailed information.
/// </summary>
/// <param name="PropertyName">The name of the property that failed validation</param>
/// <param name="ErrorMessage">The human-readable error message</param>
/// <param name="ErrorCode">An optional error code for programmatic handling</param>
/// <param name="AttemptedValue">The value that was attempted to be set</param>
public sealed record ValidationError(
    string PropertyName,
    string ErrorMessage,
    string? ErrorCode = null,
    object? AttemptedValue = null);
