namespace Mystira.Contracts.App.Responses.Common;

/// <summary>
/// Common response types shared across the Mystira App API.
/// </summary>

/// <summary>
/// Standard error response for API errors.
/// Provides consistent error structure across all Mystira services.
/// </summary>
public record ErrorResponse
{
    /// <summary>
    /// Human-readable error message.
    /// </summary>
    public required string Message { get; init; }

    /// <summary>
    /// Additional details about the error.
    /// </summary>
    public string? Details { get; init; }

    /// <summary>
    /// When the error occurred.
    /// </summary>
    public DateTimeOffset Timestamp { get; init; } = DateTimeOffset.UtcNow;

    /// <summary>
    /// Correlation ID for tracing.
    /// </summary>
    public string? TraceId { get; init; }

    /// <summary>
    /// Unique error code for lookup (e.g., "AUTH_001", "VALIDATION_002").
    /// </summary>
    public string? ErrorCode { get; init; }

    /// <summary>
    /// Error category (e.g., "Authentication", "Validation", "NotFound").
    /// </summary>
    public string? Category { get; init; }

    /// <summary>
    /// HTTP status code associated with this error.
    /// </summary>
    public int? StatusCode { get; init; }

    /// <summary>
    /// Whether the error is recoverable by the user.
    /// </summary>
    public bool IsRecoverable { get; init; } = true;

    /// <summary>
    /// Suggested action (e.g., "retry", "login", "contact-support").
    /// </summary>
    public string? SuggestedAction { get; init; }

    /// <summary>
    /// Creates a simple error response with just a message.
    /// </summary>
    public static ErrorResponse FromMessage(string message) => new() { Message = message };

    /// <summary>
    /// Creates an error response with a message and error code.
    /// </summary>
    public static ErrorResponse FromCode(string errorCode, string message) => new()
    {
        Message = message,
        ErrorCode = errorCode
    };
}

/// <summary>
/// Error response with validation errors.
/// </summary>
public record ValidationErrorResponse : ErrorResponse
{
    /// <summary>
    /// Dictionary of field names to their validation errors.
    /// </summary>
    public required IDictionary<string, IReadOnlyList<string>> Errors { get; init; }

    /// <summary>
    /// Creates a validation error response from field errors.
    /// </summary>
    public static ValidationErrorResponse FromErrors(IDictionary<string, IReadOnlyList<string>> errors) => new()
    {
        Message = "One or more validation errors occurred.",
        Errors = errors,
        Category = "Validation",
        ErrorCode = "VALIDATION_001",
        SuggestedAction = "fix-input",
        StatusCode = 400
    };
}

/// <summary>
/// Error response for not found resources.
/// </summary>
public record NotFoundErrorResponse : ErrorResponse
{
    /// <summary>
    /// Type of resource that was not found.
    /// </summary>
    public string? ResourceType { get; init; }

    /// <summary>
    /// ID of the resource that was not found.
    /// </summary>
    public string? ResourceId { get; init; }

    /// <summary>
    /// Creates a not found error response.
    /// </summary>
    public static NotFoundErrorResponse Create(string resourceType, string? resourceId = null) => new()
    {
        Message = resourceId != null
            ? $"{resourceType} with ID '{resourceId}' was not found."
            : $"{resourceType} was not found.",
        ResourceType = resourceType,
        ResourceId = resourceId,
        Category = "NotFound",
        ErrorCode = "NOT_FOUND_001",
        SuggestedAction = "check-id",
        StatusCode = 404
    };
}

/// <summary>
/// Error response for authorization failures.
/// </summary>
public record ForbiddenErrorResponse : ErrorResponse
{
    /// <summary>
    /// The permission that was required.
    /// </summary>
    public string? RequiredPermission { get; init; }

    /// <summary>
    /// Creates a forbidden error response.
    /// </summary>
    public static ForbiddenErrorResponse Create(string? requiredPermission = null) => new()
    {
        Message = requiredPermission != null
            ? $"You do not have the required permission: {requiredPermission}"
            : "You do not have permission to perform this action.",
        RequiredPermission = requiredPermission,
        Category = "Authorization",
        ErrorCode = "FORBIDDEN_001",
        SuggestedAction = "request-access",
        IsRecoverable = false,
        StatusCode = 403
    };
}

/// <summary>
/// Error response for authentication failures.
/// </summary>
public record UnauthorizedErrorResponse : ErrorResponse
{
    /// <summary>
    /// The authentication scheme that failed.
    /// </summary>
    public string? AuthScheme { get; init; }

    /// <summary>
    /// Creates an unauthorized error response.
    /// </summary>
    public static UnauthorizedErrorResponse Create(string? authScheme = null) => new()
    {
        Message = "Authentication is required to access this resource.",
        AuthScheme = authScheme,
        Category = "Authentication",
        ErrorCode = "UNAUTHORIZED_001",
        SuggestedAction = "login",
        IsRecoverable = true,
        StatusCode = 401
    };
}

/// <summary>
/// Simple validation result for field-level validation.
/// </summary>
public class ValidationResult
{
    /// <summary>
    /// Whether the validation passed.
    /// </summary>
    public bool IsValid { get; set; }

    /// <summary>
    /// Optional message describing the validation result.
    /// </summary>
    public string? Message { get; set; }

    /// <summary>
    /// Creates a successful validation result.
    /// </summary>
    public static ValidationResult Success() => new() { IsValid = true };

    /// <summary>
    /// Creates a failed validation result with a message.
    /// </summary>
    public static ValidationResult Failure(string message) => new() { IsValid = false, Message = message };
}
