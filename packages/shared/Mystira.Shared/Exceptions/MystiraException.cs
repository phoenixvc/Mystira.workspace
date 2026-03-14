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
    public IDictionary<string, object>? Details { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="MystiraException"/> class.
    /// </summary>
    /// <param name="errorCode">Unique error code for lookup.</param>
    /// <param name="message">Human-readable error message.</param>
    /// <param name="statusCode">HTTP status code to return.</param>
    /// <param name="details">Additional context data.</param>
    /// <param name="innerException">The inner exception.</param>
    public MystiraException(
        string errorCode,
        string message,
        HttpStatusCode statusCode = HttpStatusCode.InternalServerError,
        IDictionary<string, object>? details = null,
        Exception? innerException = null)
        : base(message, innerException)
    {
        ErrorCode = errorCode;
        StatusCode = statusCode;
        Details = details;
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
    private const string DefaultMessage = "One or more validation errors occurred.";

    /// <summary>
    /// Dictionary of field names to their validation errors.
    /// </summary>
    public IDictionary<string, string[]> Errors { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="ValidationException"/> class.
    /// </summary>
    /// <param name="errors">Dictionary of field names to validation error messages.</param>
    public ValidationException(IDictionary<string, string[]> errors)
        : base("VALIDATION_FAILED", DefaultMessage, HttpStatusCode.BadRequest)
    {
        Errors = errors;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ValidationException"/> class with a custom message.
    /// </summary>
    /// <param name="message">Custom error message.</param>
    /// <param name="errors">Dictionary of field names to validation error messages.</param>
    public ValidationException(string message, IDictionary<string, string[]> errors)
        : base("VALIDATION_FAILED", message, HttpStatusCode.BadRequest)
    {
        Errors = errors;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ValidationException"/> class for a single field.
    /// </summary>
    /// <param name="field">The field name.</param>
    /// <param name="error">The error message.</param>
    public ValidationException(string field, string error)
        : this(error, new Dictionary<string, string[]> { { field, [error] } })
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ValidationException"/> class with a custom message only.
    /// </summary>
    /// <param name="message">Custom error message.</param>
    public ValidationException(string message)
        : base("VALIDATION_FAILED", message, HttpStatusCode.BadRequest, new Dictionary<string, object>())
    {
        Errors = new Dictionary<string, string[]>();
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ValidationException"/> class with multiple validation errors.
    /// </summary>
    /// <param name="errors">Array of validation errors.</param>
    public ValidationException(IEnumerable<ValidationError> errors)
        : base("VALIDATION_FAILED", DefaultMessage, HttpStatusCode.BadRequest, new Dictionary<string, object>
        {
            ["errors"] = errors.ToArray()
        })
    {
        Errors = errors.GroupBy(e => e.Field)
                      .ToDictionary(g => g.Key, g => g.Select(e => e.Message).ToArray());
    }
}

/// <summary>
/// Represents a single validation error.
/// </summary>
public record ValidationError(string Field, string Message);

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
        : base("RESOURCE_NOT_FOUND",
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

    /// <summary>
    /// Initializes a new instance of the <see cref="NotFoundException"/> class with a custom message.
    /// </summary>
    /// <param name="resourceType">The type of resource that was not found.</param>
    /// <param name="resourceId">The ID of the resource that was not found.</param>
    /// <param name="message">Custom error message.</param>
    public NotFoundException(string resourceType, string? resourceId, string message)
        : base("RESOURCE_NOT_FOUND", message, HttpStatusCode.NotFound, new Dictionary<string, object>
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
    /// The resource that access was forbidden for.
    /// </summary>
    public string? Resource { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="ForbiddenException"/> class.
    /// </summary>
    /// <param name="message">Custom error message.</param>
    public ForbiddenException(string message)
        : base("ACCESS_FORBIDDEN", message, HttpStatusCode.Forbidden)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ForbiddenException"/> class with resource context.
    /// </summary>
    /// <param name="resource">The resource being accessed.</param>
    /// <param name="message">Custom error message.</param>
    public ForbiddenException(string resource, string message)
        : base("ACCESS_FORBIDDEN", message, HttpStatusCode.Forbidden, new Dictionary<string, object>
        {
            ["resource"] = resource
        })
    {
        Resource = resource;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ForbiddenException"/> class with resource and permission context.
    /// </summary>
    /// <param name="resource">The resource being accessed.</param>
    /// <param name="requiredPermission">The required permission.</param>
    /// <param name="message">Custom error message.</param>
    public ForbiddenException(string resource, string requiredPermission, string message)
        : base("ACCESS_FORBIDDEN", message, HttpStatusCode.Forbidden, new Dictionary<string, object>
        {
            ["resource"] = resource,
            ["requiredPermission"] = requiredPermission
        })
    {
        Resource = resource;
        RequiredPermission = requiredPermission;
    }
}

/// <summary>
/// Exception for conflict errors (e.g., duplicate resources).
/// </summary>
public class ConflictException : MystiraException
{
    /// <summary>
    /// The type of resource that caused the conflict.
    /// </summary>
    public string ResourceType { get; } = "Unknown";

    /// <summary>
    /// The field that caused the conflict.
    /// </summary>
    public string? ConflictingField { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="ConflictException"/> class.
    /// </summary>
    /// <param name="message">The conflict error message.</param>
    public ConflictException(string message)
        : base("RESOURCE_CONFLICT", message, HttpStatusCode.Conflict)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ConflictException"/> class with resource type.
    /// </summary>
    /// <param name="resourceType">The resource type.</param>
    /// <param name="message">The conflict error message.</param>
    public ConflictException(string resourceType, string message)
        : base("RESOURCE_CONFLICT", message, HttpStatusCode.Conflict, new Dictionary<string, object>
        {
            ["resourceType"] = resourceType
        })
    {
        ResourceType = resourceType;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ConflictException"/> class with resource type and field.
    /// </summary>
    /// <param name="resourceType">The resource type.</param>
    /// <param name="conflictingField">The conflicting field.</param>
    /// <param name="message">The conflict error message.</param>
    public ConflictException(string resourceType, string conflictingField, string message)
        : base("RESOURCE_CONFLICT", message, HttpStatusCode.Conflict, new Dictionary<string, object>
        {
            ["resourceType"] = resourceType,
            ["conflictingField"] = conflictingField
        })
    {
        ResourceType = resourceType;
        ConflictingField = conflictingField;
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
/// Exception for business rule violations.
/// </summary>
public class BusinessRuleException : MystiraException
{
    /// <summary>
    /// The business rule that was violated.
    /// </summary>
    public string RuleName { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="BusinessRuleException"/> class.
    /// </summary>
    /// <param name="ruleName">The business rule that was violated.</param>
    /// <param name="message">The error message.</param>
    public BusinessRuleException(string ruleName, string message)
        : base("BUSINESS_RULE_VIOLATION", message, HttpStatusCode.UnprocessableEntity, new Dictionary<string, object>
        {
            ["ruleName"] = ruleName
        })
    {
        RuleName = ruleName;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="BusinessRuleException"/> class with context.
    /// </summary>
    /// <param name="ruleName">The business rule that was violated.</param>
    /// <param name="message">The error message.</param>
    /// <param name="context">Additional context data.</param>
    public BusinessRuleException(string ruleName, string message, IDictionary<string, object> context)
        : base("BUSINESS_RULE_VIOLATION", message, HttpStatusCode.UnprocessableEntity, new Dictionary<string, object>(context)
        {
            ["ruleName"] = ruleName
        })
    {
        RuleName = ruleName;
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
