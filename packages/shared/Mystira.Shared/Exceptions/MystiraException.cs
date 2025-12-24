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
    public IDictionary<string, string[]> Errors { get; }

    public ValidationException(IDictionary<string, string[]> errors)
        : base("VALIDATION_FAILED", "One or more validation errors occurred.",
            HttpStatusCode.BadRequest)
    {
        Errors = errors;
    }

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
    public string ResourceType { get; }
    public string? ResourceId { get; }

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
    public string? RequiredPermission { get; }

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
    public string ServiceName { get; }

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
    public TimeSpan? RetryAfter { get; }

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
