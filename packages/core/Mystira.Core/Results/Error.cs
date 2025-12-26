namespace Mystira.Core.Results;

/// <summary>
/// Represents an error in the Result pattern.
/// This is for internal error handling, not for API responses.
/// </summary>
/// <param name="Code">Unique error code for programmatic handling.</param>
/// <param name="Message">Human-readable error message.</param>
/// <param name="Exception">Optional underlying exception.</param>
public sealed record Error(string Code, string Message, Exception? Exception = null)
{
    /// <summary>
    /// Additional metadata about the error.
    /// </summary>
    public IReadOnlyDictionary<string, object>? Metadata { get; init; }

    /// <summary>
    /// Creates an error with metadata.
    /// </summary>
    public Error WithMetadata(string key, object value)
    {
        var existingMetadata = Metadata ?? new Dictionary<string, object>();
        var newMetadata = new Dictionary<string, object>(existingMetadata) { [key] = value };
        return this with { Metadata = newMetadata };
    }

    /// <summary>
    /// Creates a "not found" error.
    /// </summary>
    public static Error NotFound(string resource, string? id = null) =>
        new("NOT_FOUND", id is null ? $"{resource} not found" : $"{resource} '{id}' not found");

    /// <summary>
    /// Creates a validation error.
    /// </summary>
    public static Error Validation(string message) =>
        new("VALIDATION", message);

    /// <summary>
    /// Creates a validation error with field-level details.
    /// </summary>
    public static Error Validation(string field, string message) =>
        new Error("VALIDATION", message).WithMetadata("field", field);

    /// <summary>
    /// Creates an "unauthorized" error.
    /// </summary>
    public static Error Unauthorized(string? reason = null) =>
        new("UNAUTHORIZED", reason ?? "Authentication required");

    /// <summary>
    /// Creates a "forbidden" error.
    /// </summary>
    public static Error Forbidden(string? permission = null) =>
        new("FORBIDDEN", permission is null ? "Access denied" : $"Missing permission: {permission}");

    /// <summary>
    /// Creates a "conflict" error.
    /// </summary>
    public static Error Conflict(string message) =>
        new("CONFLICT", message);

    /// <summary>
    /// Creates an "internal" error, optionally with the underlying exception.
    /// </summary>
    public static Error Internal(string message, Exception? ex = null) =>
        new("INTERNAL", message, ex);

    /// <summary>
    /// Creates an error for an invalid operation.
    /// </summary>
    public static Error InvalidOperation(string message) =>
        new("INVALID_OPERATION", message);

    /// <summary>
    /// Creates an error for when a precondition fails.
    /// </summary>
    public static Error PreconditionFailed(string message) =>
        new("PRECONDITION_FAILED", message);

    /// <summary>
    /// Creates a rate limit error.
    /// </summary>
    public static Error RateLimitExceeded(TimeSpan? retryAfter = null)
    {
        var error = new Error("RATE_LIMIT", "Rate limit exceeded");
        return retryAfter.HasValue
            ? error.WithMetadata("retryAfterSeconds", retryAfter.Value.TotalSeconds)
            : error;
    }

    /// <summary>
    /// Creates an error from an exception.
    /// </summary>
    public static Error FromException(Exception ex) =>
        new("EXCEPTION", ex.Message, ex);

    /// <inheritdoc />
    public override string ToString() =>
        Exception is null ? $"[{Code}] {Message}" : $"[{Code}] {Message} (Exception: {Exception.GetType().Name})";
}
