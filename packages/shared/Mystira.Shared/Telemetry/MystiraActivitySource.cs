using System.Diagnostics;

namespace Mystira.Shared.Telemetry;

/// <summary>
/// Central ActivitySource for Mystira platform tracing.
/// Use this for creating spans for repository operations, caching, and messaging.
/// </summary>
public static class MystiraActivitySource
{
    /// <summary>
    /// The name of the activity source used for Mystira tracing.
    /// </summary>
    public const string Name = "Mystira.Shared";

    /// <summary>
    /// The version of the activity source.
    /// </summary>
    public const string Version = "0.1.0";

    /// <summary>
    /// The activity source for Mystira operations.
    /// </summary>
    public static readonly ActivitySource Source = new(Name, Version);

    /// <summary>
    /// Starts a new activity for a repository operation.
    /// </summary>
    /// <param name="operationName">The name of the operation (e.g., "GetById", "Add", "Update").</param>
    /// <param name="entityType">The entity type being operated on.</param>
    /// <param name="kind">The kind of activity.</param>
    /// <returns>The started activity, or null if tracing is not enabled.</returns>
    public static Activity? StartRepositoryActivity(
        string operationName,
        string entityType,
        ActivityKind kind = ActivityKind.Internal)
    {
        var activity = Source.StartActivity($"Repository.{operationName}", kind);
        if (activity != null)
        {
            activity.SetTag("db.system", "entityframework");
            activity.SetTag("db.operation", operationName);
            activity.SetTag("mystira.entity_type", entityType);
        }
        return activity;
    }

    /// <summary>
    /// Starts a new activity for a cache operation.
    /// </summary>
    /// <param name="operationName">The name of the operation (e.g., "Get", "Set", "Remove").</param>
    /// <param name="cacheKey">The cache key being operated on.</param>
    /// <param name="kind">The kind of activity.</param>
    /// <returns>The started activity, or null if tracing is not enabled.</returns>
    public static Activity? StartCacheActivity(
        string operationName,
        string cacheKey,
        ActivityKind kind = ActivityKind.Internal)
    {
        var activity = Source.StartActivity($"Cache.{operationName}", kind);
        if (activity != null)
        {
            activity.SetTag("cache.operation", operationName);
            activity.SetTag("cache.key", cacheKey);
        }
        return activity;
    }

    /// <summary>
    /// Starts a new activity for a messaging operation.
    /// </summary>
    /// <param name="operationName">The name of the operation (e.g., "Publish", "Send", "Handle").</param>
    /// <param name="messageType">The type of message being processed.</param>
    /// <param name="kind">The kind of activity.</param>
    /// <returns>The started activity, or null if tracing is not enabled.</returns>
    public static Activity? StartMessagingActivity(
        string operationName,
        string messageType,
        ActivityKind kind = ActivityKind.Producer)
    {
        var activity = Source.StartActivity($"Messaging.{operationName}", kind);
        if (activity != null)
        {
            activity.SetTag("messaging.system", "wolverine");
            activity.SetTag("messaging.operation", operationName);
            activity.SetTag("messaging.message_type", messageType);
        }
        return activity;
    }

    /// <summary>
    /// Records an exception on the current activity.
    /// </summary>
    /// <param name="activity">The activity to record on.</param>
    /// <param name="exception">The exception to record.</param>
    public static void RecordException(this Activity? activity, Exception exception)
    {
        if (activity == null) return;

        activity.SetStatus(ActivityStatusCode.Error, exception.Message);
        activity.AddEvent(new ActivityEvent("exception", tags: new ActivityTagsCollection
        {
            ["exception.type"] = exception.GetType().FullName,
            ["exception.message"] = exception.Message,
            ["exception.stacktrace"] = exception.StackTrace
        }));
    }

    /// <summary>
    /// Records a cache hit or miss on the activity.
    /// </summary>
    /// <param name="activity">The activity to record on.</param>
    /// <param name="hit">True if cache hit, false if miss.</param>
    public static void RecordCacheResult(this Activity? activity, bool hit)
    {
        activity?.SetTag("cache.hit", hit);
    }
}
