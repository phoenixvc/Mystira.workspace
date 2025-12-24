using System.Diagnostics;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Mystira.Shared.Exceptions;

/// <summary>
/// Global exception handler that converts exceptions to ProblemDetails responses.
/// Implements IExceptionHandler for ASP.NET Core 8+ exception handling.
/// </summary>
public class GlobalExceptionHandler : IExceptionHandler
{
    private readonly ILogger<GlobalExceptionHandler> _logger;
    private readonly IHostEnvironment _environment;

    public GlobalExceptionHandler(
        ILogger<GlobalExceptionHandler> logger,
        IHostEnvironment environment)
    {
        _logger = logger;
        _environment = environment;
    }

    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        var traceId = Activity.Current?.Id ?? httpContext.TraceIdentifier;

        _logger.LogError(exception,
            "Unhandled exception occurred. TraceId: {TraceId}, Path: {Path}",
            traceId, httpContext.Request.Path);

        var problemDetails = CreateProblemDetails(exception, traceId);

        httpContext.Response.StatusCode = problemDetails.Status ?? StatusCodes.Status500InternalServerError;
        httpContext.Response.ContentType = "application/problem+json";

        await httpContext.Response.WriteAsJsonAsync(problemDetails, cancellationToken);

        return true;
    }

    private ProblemDetails CreateProblemDetails(Exception exception, string traceId)
    {
        var includeDetails = _environment.IsDevelopment();

        return exception switch
        {
            ValidationException validationEx => new ValidationProblemDetails(validationEx.Errors)
            {
                Status = StatusCodes.Status400BadRequest,
                Title = "Validation Failed",
                Detail = validationEx.Message,
                Instance = traceId,
                Extensions = { ["errorCode"] = validationEx.ErrorCode }
            },

            NotFoundException notFoundEx => new ProblemDetails
            {
                Status = StatusCodes.Status404NotFound,
                Title = "Resource Not Found",
                Detail = notFoundEx.Message,
                Instance = traceId,
                Extensions =
                {
                    ["errorCode"] = notFoundEx.ErrorCode,
                    ["resourceType"] = notFoundEx.ResourceType,
                    ["resourceId"] = notFoundEx.ResourceId ?? ""
                }
            },

            UnauthorizedException unauthorizedEx => new ProblemDetails
            {
                Status = StatusCodes.Status401Unauthorized,
                Title = "Unauthorized",
                Detail = unauthorizedEx.Message,
                Instance = traceId,
                Extensions = { ["errorCode"] = unauthorizedEx.ErrorCode }
            },

            ForbiddenException forbiddenEx => new ProblemDetails
            {
                Status = StatusCodes.Status403Forbidden,
                Title = "Forbidden",
                Detail = forbiddenEx.Message,
                Instance = traceId,
                Extensions =
                {
                    ["errorCode"] = forbiddenEx.ErrorCode,
                    ["requiredPermission"] = forbiddenEx.RequiredPermission ?? ""
                }
            },

            ConflictException conflictEx => new ProblemDetails
            {
                Status = StatusCodes.Status409Conflict,
                Title = "Conflict",
                Detail = conflictEx.Message,
                Instance = traceId,
                Extensions = { ["errorCode"] = conflictEx.ErrorCode }
            },

            ServiceUnavailableException serviceEx => new ProblemDetails
            {
                Status = StatusCodes.Status503ServiceUnavailable,
                Title = "Service Unavailable",
                Detail = serviceEx.Message,
                Instance = traceId,
                Extensions =
                {
                    ["errorCode"] = serviceEx.ErrorCode,
                    ["serviceName"] = serviceEx.ServiceName
                }
            },

            RateLimitedException rateLimitEx => new ProblemDetails
            {
                Status = StatusCodes.Status429TooManyRequests,
                Title = "Rate Limit Exceeded",
                Detail = rateLimitEx.Message,
                Instance = traceId,
                Extensions =
                {
                    ["errorCode"] = rateLimitEx.ErrorCode,
                    ["retryAfterSeconds"] = rateLimitEx.RetryAfter?.TotalSeconds ?? 60
                }
            },

            MystiraException mystiraEx => new ProblemDetails
            {
                Status = (int)mystiraEx.StatusCode,
                Title = mystiraEx.ErrorCode,
                Detail = mystiraEx.Message,
                Instance = traceId,
                Extensions = { ["errorCode"] = mystiraEx.ErrorCode }
            },

            OperationCanceledException => new ProblemDetails
            {
                Status = StatusCodes.Status499ClientClosedRequest,
                Title = "Request Cancelled",
                Detail = "The request was cancelled by the client.",
                Instance = traceId,
                Extensions = { ["errorCode"] = "REQUEST_CANCELLED" }
            },

            _ => new ProblemDetails
            {
                Status = StatusCodes.Status500InternalServerError,
                Title = "Internal Server Error",
                Detail = includeDetails ? exception.Message : "An unexpected error occurred.",
                Instance = traceId,
                Extensions =
                {
                    ["errorCode"] = "INTERNAL_ERROR",
                    ["exceptionType"] = includeDetails ? exception.GetType().Name : "Exception"
                }
            }
        };
    }
}
