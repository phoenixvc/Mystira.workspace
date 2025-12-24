namespace Mystira.Contracts.App;

/// <summary>
/// App API Contracts
///
/// These types will be migrated from Mystira.App.Contracts.
/// During migration, types will be added here and deprecated in the old package.
///
/// Migration status: PENDING
/// See: docs/architecture/adr/0020-package-consolidation-strategy.md
/// </summary>

/// <summary>
/// Base request interface for API calls
/// </summary>
public record ApiRequest
{
    /// <summary>
    /// Request correlation ID for tracing
    /// </summary>
    public string? CorrelationId { get; init; }
}

/// <summary>
/// Base response interface for API calls
/// </summary>
/// <typeparam name="T">Type of response data</typeparam>
public record ApiResponse<T>
{
    /// <summary>
    /// Whether the request was successful
    /// </summary>
    public required bool Success { get; init; }

    /// <summary>
    /// Response data
    /// </summary>
    public T? Data { get; init; }

    /// <summary>
    /// Error details if not successful
    /// </summary>
    public ApiError? Error { get; init; }
}

/// <summary>
/// API error details
/// </summary>
public record ApiError
{
    /// <summary>
    /// Error code
    /// </summary>
    public required string Code { get; init; }

    /// <summary>
    /// Human-readable error message
    /// </summary>
    public required string Message { get; init; }

    /// <summary>
    /// Additional error details
    /// </summary>
    public IDictionary<string, object>? Details { get; init; }
}

/// <summary>
/// Story creation request
/// </summary>
/// <remarks>
/// Placeholder - will be replaced during migration from Mystira.App.Contracts
/// </remarks>
public record StoryRequest : ApiRequest
{
    /// <summary>
    /// Story title
    /// </summary>
    public required string Title { get; init; }

    /// <summary>
    /// Story content
    /// </summary>
    public required string Content { get; init; }

    /// <summary>
    /// Additional metadata
    /// </summary>
    public IDictionary<string, object>? Metadata { get; init; }
}

/// <summary>
/// Story response
/// </summary>
/// <remarks>
/// Placeholder - will be replaced during migration from Mystira.App.Contracts
/// </remarks>
public record StoryResponse
{
    /// <summary>
    /// Unique story identifier
    /// </summary>
    public required string Id { get; init; }

    /// <summary>
    /// Story title
    /// </summary>
    public required string Title { get; init; }

    /// <summary>
    /// Story content
    /// </summary>
    public required string Content { get; init; }

    /// <summary>
    /// Creation timestamp
    /// </summary>
    public required DateTimeOffset CreatedAt { get; init; }

    /// <summary>
    /// Last update timestamp
    /// </summary>
    public required DateTimeOffset UpdatedAt { get; init; }
}
