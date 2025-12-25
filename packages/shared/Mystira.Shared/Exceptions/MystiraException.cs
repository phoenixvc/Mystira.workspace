using System.Net;

namespace Mystira.Shared.Exceptions;

/// <summary>
/// Base exception type for Mystira platform errors.
/// Provides structured error information for consistent handling.
/// </summary>
public class MystiraException : Exception
{
    /// <summary>
    /// Unique error code for lookup.
    /// </summary>
    public string ErrorCode { get; }

    /// <summary>
    /// HTTP status code to return for this error.
    /// </summary>
    public HttpStatusCode StatusCode { get; }

    /// <summary>
    /// Additional context data for the error.
    /// </summary>
    public IDictionary<string, object>? Context { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="MystiraException"/> class.
    /// </summary>
    /// <param name="errorCode">Unique error code for lookup.</param>
    /// <param name="message">Human-readable error message.</param>
    /// <param name="statusCode">HTTP status code to return.</param>
    /// <param name="context">Additional context data.</param>
    /// <param name="innerException">The inner exception.</param>
    public MystiraException(
        string errorCode,
        string message,
        HttpStatusCode statusCode = HttpStatusCode.InternalServerError,
        IDictionary<string, object>? context = null,
        Exception? innerException = null)
        : base(message, innerException)
    {
        ErrorCode = errorCode;
        StatusCode = statusCode;
        Context = context;
    }

    /// <summary>
    /// Converts to an ErrorResponse for API responses.
    /// </summary>
    public ErrorResponse ToErrorResponse(bool includeDetails = false)
    {
        return new ErrorResponse
        {
            Message = Message,
            ErrorCode = ErrorCode,
            Category = GetType().Name.Replace("Exception", ""),
            Details = includeDetails ? ToString() : null,
            IsRecoverable = StatusCode is not (
                HttpStatusCode.InternalServerError or
                HttpStatusCode.ServiceUnavailable)
        };
    }
}

/// <summary>
/// Exception for validation errors.
/// </summary>
public class ValidationException : MystiraException
{
    /// <summary>
    /// Dictionary of field names to their validation errors.
    /// </summary>
    public IDictionary<string, string[]> Errors { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="ValidationException"/> class.
    /// </summary>
    /// <param name="errors">Dictionary of field names to validation error messages.</param>
    public ValidationException(IDictionary<string, string[]> errors)
        : base("VALIDATION_FAILED", "One or more validation errors occurred.",
            HttpStatusCode.BadRequest)
    {
        Errors = errors;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ValidationException"/> class for a single field.
    /// </summary>
    /// <param name="field">The field name.</param>
    /// <param name="error">The error message.</param>
    public ValidationException(string field, string error)
        : this(new Dictionary<string, string[]> { { field, new[] { error } } })
    {
    }
}

/// <summary>
/// Exception for not found resources.
/// </summary>
public class NotFoundException : MystiraException
{
    /// <summary>
    /// The type of resource that was not found.
    /// </summary>
    public string ResourceType { get; }

    /// <summary>
    /// The ID of the resource that was not found.
    /// </summary>
    public string? ResourceId { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="NotFoundException"/> class.
    /// </summary>
    /// <param name="resourceType">The type of resource that was not found.</param>
    /// <param name="resourceId">The ID of the resource that was not found.</param>
    public NotFoundException(string resourceType, string? resourceId = null)
        : base("NOT_FOUND",
            resourceId != null
                ? $"{resourceType} with ID '{resourceId}' was not found."
                : $"{resourceType} was not found.",
            HttpStatusCode.NotFound,
            new Dictionary<string, object>
            {
                ["resourceType"] = resourceType,
                ["resourceId"] = resourceId ?? ""
            })
    {
        ResourceType = resourceType;
        ResourceId = resourceId;
    }
}

/// <summary>
/// Exception for unauthorized access.
/// </summary>
public class UnauthorizedException : MystiraException
{
    /// <summary>
    /// Initializes a new instance of the <see cref="UnauthorizedException"/> class.
    /// </summary>
    /// <param name="message">Optional custom message.</param>
    public UnauthorizedException(string? message = null)
        : base("UNAUTHORIZED",
            message ?? "Authentication is required.",
            HttpStatusCode.Unauthorized)
    {
    }
}

/// <summary>
/// Exception for forbidden access.
/// </summary>
public class ForbiddenException : MystiraException
{
    /// <summary>
    /// The permission that was required but not granted.
    /// </summary>
    public string? RequiredPermission { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="ForbiddenException"/> class.
    /// </summary>
    /// <param name="requiredPermission">The permission that was required.</param>
    /// <param name="message">Optional custom message.</param>
    public ForbiddenException(string? requiredPermission = null, string? message = null)
        : base("FORBIDDEN",
            message ?? (requiredPermission != null
                ? $"You do not have the required permission: {requiredPermission}"
                : "You do not have permission to perform this action."),
            HttpStatusCode.Forbidden)
    {
        RequiredPermission = requiredPermission;
    }
}

/// <summary>
/// Exception for conflict errors (e.g., duplicate resources).
/// </summary>
public class ConflictException : MystiraException
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ConflictException"/> class.
    /// </summary>
    /// <param name="message">The conflict error message.</param>
    public ConflictException(string message)
        : base("CONFLICT", message, HttpStatusCode.Conflict)
    {
    }
}

/// <summary>
/// Exception for service unavailable errors.
/// </summary>
public class ServiceUnavailableException : MystiraException
{
    /// <summary>
    /// The name of the unavailable service.
    /// </summary>
    public string ServiceName { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="ServiceUnavailableException"/> class.
    /// </summary>
    /// <param name="serviceName">The name of the unavailable service.</param>
    /// <param name="message">Optional custom message.</param>
    public ServiceUnavailableException(string serviceName, string? message = null)
        : base("SERVICE_UNAVAILABLE",
            message ?? $"The {serviceName} service is currently unavailable.",
            HttpStatusCode.ServiceUnavailable)
    {
        ServiceName = serviceName;
    }
}

/// <summary>
/// Exception for rate limiting.
/// </summary>
public class RateLimitedException : MystiraException
{
    /// <summary>
    /// The time to wait before retrying.
    /// </summary>
    public TimeSpan? RetryAfter { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="RateLimitedException"/> class.
    /// </summary>
    /// <param name="retryAfter">The time to wait before retrying.</param>
    public RateLimitedException(TimeSpan? retryAfter = null)
        : base("RATE_LIMITED",
            retryAfter.HasValue
                ? $"Rate limit exceeded. Please retry after {retryAfter.Value.TotalSeconds} seconds."
                : "Rate limit exceeded. Please try again later.",
            HttpStatusCode.TooManyRequests)
    {
        RetryAfter = retryAfter;
    }
}
