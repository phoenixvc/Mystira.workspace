using ContractsErrorResponse = Mystira.Contracts.App.Responses.Common.ErrorResponse;
using ContractsValidationErrorResponse = Mystira.Contracts.App.Responses.Common.ValidationErrorResponse;
using ContractsNotFoundErrorResponse = Mystira.Contracts.App.Responses.Common.NotFoundErrorResponse;
using ContractsForbiddenErrorResponse = Mystira.Contracts.App.Responses.Common.ForbiddenErrorResponse;

namespace Mystira.Shared.Exceptions;

// ============================================================================
// DEPRECATION NOTICE
// ============================================================================
// These ErrorResponse types are deprecated. Use Mystira.Contracts types instead:
//
//   using Mystira.Contracts.App.Responses.Common;
//
//   - ErrorResponse              -> Mystira.Contracts.App.Responses.Common.ErrorResponse
//   - ValidationErrorResponse    -> Mystira.Contracts.App.Responses.Common.ValidationErrorResponse
//   - NotFoundErrorResponse      -> Mystira.Contracts.App.Responses.Common.NotFoundErrorResponse
//   - ForbiddenErrorResponse     -> Mystira.Contracts.App.Responses.Common.ForbiddenErrorResponse
//
// The Contracts versions are immutable records with init-only properties.
// These classes are retained for backward compatibility only.
// ============================================================================

/// <summary>
/// Standard error response for API errors.
/// Provides consistent error structure across all Mystira services.
/// </summary>
/// <remarks>
/// DEPRECATED: Use <see cref="Mystira.Contracts.App.Responses.Common.ErrorResponse"/> instead.
/// </remarks>
[Obsolete("Use Mystira.Contracts.App.Responses.Common.ErrorResponse instead. This type will be removed in a future version.")]
public class ErrorResponse
{
    /// <summary>
    /// Human-readable error message.
    /// </summary>
    public string Message { get; set; } = string.Empty;

    /// <summary>
    /// Additional details about the error.
    /// </summary>
    public string? Details { get; set; }

    /// <summary>
    /// When the error occurred.
    /// </summary>
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Correlation ID for tracing.
    /// </summary>
    public string? TraceId { get; set; }

    /// <summary>
    /// Unique error code for lookup (e.g., "AUTH_001", "VALIDATION_002").
    /// </summary>
    public string? ErrorCode { get; set; }

    /// <summary>
    /// Error category (e.g., "Authentication", "Validation", "NotFound").
    /// </summary>
    public string? Category { get; set; }

    /// <summary>
    /// Whether the error is recoverable by the user.
    /// </summary>
    public bool IsRecoverable { get; set; } = true;

    /// <summary>
    /// Suggested action (e.g., "retry", "login", "contact-support").
    /// </summary>
    public string? SuggestedAction { get; set; }

    /// <summary>
    /// Creates an error response from an exception.
    /// </summary>
    public static ErrorResponse FromException(Exception ex, bool includeDetails = false)
    {
        return new ErrorResponse
        {
            Message = ex.Message,
            Details = includeDetails ? FormatExceptionDetails(ex) : null,
            Category = ex.GetType().Name.Replace("Exception", ""),
            IsRecoverable = ex is not (OutOfMemoryException or StackOverflowException)
        };
    }

    private static string FormatExceptionDetails(Exception ex)
    {
        var details = $"{ex.GetType().Name}: {ex.Message}";
        if (ex.InnerException != null)
        {
            details += $" | Inner: {ex.InnerException.GetType().Name}: {ex.InnerException.Message}";
        }
        return details;
    }
}

/// <summary>
/// Error response with validation errors.
/// </summary>
/// <remarks>
/// DEPRECATED: Use <see cref="Mystira.Contracts.App.Responses.Common.ValidationErrorResponse"/> instead.
/// </remarks>
[Obsolete("Use Mystira.Contracts.App.Responses.Common.ValidationErrorResponse instead. This type will be removed in a future version.")]
public class ValidationErrorResponse : ErrorResponse
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ValidationErrorResponse"/> class.
    /// </summary>
    public ValidationErrorResponse()
    {
        Category = "Validation";
        ErrorCode = "VALIDATION_001";
        SuggestedAction = "fix-input";
    }

    /// <summary>
    /// Dictionary of field names to their validation errors.
    /// </summary>
    public Dictionary<string, List<string>> Errors { get; set; } = new();
}

/// <summary>
/// Error response for not found resources.
/// </summary>
/// <remarks>
/// DEPRECATED: Use <see cref="Mystira.Contracts.App.Responses.Common.NotFoundErrorResponse"/> instead.
/// </remarks>
[Obsolete("Use Mystira.Contracts.App.Responses.Common.NotFoundErrorResponse instead. This type will be removed in a future version.")]
public class NotFoundErrorResponse : ErrorResponse
{
    /// <summary>
    /// Initializes a new instance of the <see cref="NotFoundErrorResponse"/> class.
    /// </summary>
    /// <param name="resourceType">The type of resource that was not found.</param>
    /// <param name="resourceId">The ID of the resource that was not found.</param>
    public NotFoundErrorResponse(string resourceType, string? resourceId = null)
    {
        Category = "NotFound";
        ErrorCode = "NOT_FOUND_001";
        Message = resourceId != null
            ? $"{resourceType} with ID '{resourceId}' was not found."
            : $"{resourceType} was not found.";
        IsRecoverable = true;
        SuggestedAction = "check-id";
        ResourceType = resourceType;
        ResourceId = resourceId;
    }

    /// <summary>
    /// Type of resource that was not found.
    /// </summary>
    public string? ResourceType { get; set; }

    /// <summary>
    /// ID of the resource that was not found.
    /// </summary>
    public string? ResourceId { get; set; }
}

/// <summary>
/// Error response for authorization failures.
/// </summary>
/// <remarks>
/// DEPRECATED: Use <see cref="Mystira.Contracts.App.Responses.Common.ForbiddenErrorResponse"/> instead.
/// </remarks>
[Obsolete("Use Mystira.Contracts.App.Responses.Common.ForbiddenErrorResponse instead. This type will be removed in a future version.")]
public class ForbiddenErrorResponse : ErrorResponse
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ForbiddenErrorResponse"/> class.
    /// </summary>
    /// <param name="requiredPermission">The permission that was required but not granted.</param>
    public ForbiddenErrorResponse(string? requiredPermission = null)
    {
        Category = "Authorization";
        ErrorCode = "FORBIDDEN_001";
        Message = requiredPermission != null
            ? $"You do not have the required permission: {requiredPermission}"
            : "You do not have permission to perform this action.";
        IsRecoverable = false;
        SuggestedAction = "request-access";
    }
}
