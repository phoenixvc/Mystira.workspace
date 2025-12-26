namespace Mystira.Shared.Messaging.Events;

/// <summary>
/// Published when a search is performed.
/// </summary>
public sealed record SearchPerformed : IntegrationEventBase
{
    /// <summary>
    /// The search ID.
    /// </summary>
    public required string SearchId { get; init; }

    /// <summary>
    /// The user's account ID.
    /// </summary>
    public required string AccountId { get; init; }

    /// <summary>
    /// Search query (sanitized/hashed for privacy).
    /// </summary>
    public required string QueryHash { get; init; }

    /// <summary>
    /// Search type (scenarios, users, tags, all).
    /// </summary>
    public required string SearchType { get; init; }

    /// <summary>
    /// Number of results returned.
    /// </summary>
    public required int ResultCount { get; init; }

    /// <summary>
    /// Search latency in milliseconds.
    /// </summary>
    public int LatencyMs { get; init; }

    /// <summary>
    /// Filters applied.
    /// </summary>
    public string[]? Filters { get; init; }
}

/// <summary>
/// Published when a search result is clicked.
/// </summary>
public sealed record SearchResultClicked : IntegrationEventBase
{
    /// <summary>
    /// The search ID.
    /// </summary>
    public required string SearchId { get; init; }

    /// <summary>
    /// The user's account ID.
    /// </summary>
    public required string AccountId { get; init; }

    /// <summary>
    /// Result type (scenario, user, tag).
    /// </summary>
    public required string ResultType { get; init; }

    /// <summary>
    /// Result ID.
    /// </summary>
    public required string ResultId { get; init; }

    /// <summary>
    /// Position in results (1-indexed).
    /// </summary>
    public required int Position { get; init; }
}

/// <summary>
/// Published when a recommendation is shown.
/// </summary>
public sealed record RecommendationShown : IntegrationEventBase
{
    /// <summary>
    /// Recommendation batch ID.
    /// </summary>
    public required string RecommendationId { get; init; }

    /// <summary>
    /// The user's account ID.
    /// </summary>
    public required string AccountId { get; init; }

    /// <summary>
    /// Recommendation type (for_you, similar, trending, new).
    /// </summary>
    public required string RecommendationType { get; init; }

    /// <summary>
    /// Recommended item IDs.
    /// </summary>
    public required string[] ItemIds { get; init; }

    /// <summary>
    /// Algorithm version used.
    /// </summary>
    public required string AlgorithmVersion { get; init; }

    /// <summary>
    /// Context (home, scenario_end, browse).
    /// </summary>
    public required string Context { get; init; }
}

/// <summary>
/// Published when a recommendation is clicked.
/// </summary>
public sealed record RecommendationClicked : IntegrationEventBase
{
    /// <summary>
    /// Recommendation batch ID.
    /// </summary>
    public required string RecommendationId { get; init; }

    /// <summary>
    /// The user's account ID.
    /// </summary>
    public required string AccountId { get; init; }

    /// <summary>
    /// Clicked item ID.
    /// </summary>
    public required string ItemId { get; init; }

    /// <summary>
    /// Position in recommendations.
    /// </summary>
    public required int Position { get; init; }

    /// <summary>
    /// Recommendation type.
    /// </summary>
    public required string RecommendationType { get; init; }
}

/// <summary>
/// Published when a recommendation is dismissed.
/// </summary>
public sealed record RecommendationDismissed : IntegrationEventBase
{
    /// <summary>
    /// Recommendation batch ID.
    /// </summary>
    public required string RecommendationId { get; init; }

    /// <summary>
    /// The user's account ID.
    /// </summary>
    public required string AccountId { get; init; }

    /// <summary>
    /// Dismissed item ID.
    /// </summary>
    public required string ItemId { get; init; }

    /// <summary>
    /// Reason if provided (not_interested, already_played, inappropriate).
    /// </summary>
    public string? Reason { get; init; }
}

/// <summary>
/// Published when content is featured/highlighted.
/// </summary>
public sealed record ContentFeatured : IntegrationEventBase
{
    /// <summary>
    /// Content type (scenario, user, collection).
    /// </summary>
    public required string ContentType { get; init; }

    /// <summary>
    /// Content ID.
    /// </summary>
    public required string ContentId { get; init; }

    /// <summary>
    /// Feature location (home_hero, category_top, editor_pick).
    /// </summary>
    public required string FeatureLocation { get; init; }

    /// <summary>
    /// Start time of featuring.
    /// </summary>
    public required DateTimeOffset StartsAt { get; init; }

    /// <summary>
    /// End time of featuring.
    /// </summary>
    public required DateTimeOffset EndsAt { get; init; }
}

/// <summary>
/// Published when a tag is followed.
/// </summary>
public sealed record TagFollowed : IntegrationEventBase
{
    /// <summary>
    /// The user's account ID.
    /// </summary>
    public required string AccountId { get; init; }

    /// <summary>
    /// Tag ID.
    /// </summary>
    public required string TagId { get; init; }

    /// <summary>
    /// Tag name.
    /// </summary>
    public required string TagName { get; init; }
}

/// <summary>
/// Published when a tag is unfollowed.
/// </summary>
public sealed record TagUnfollowed : IntegrationEventBase
{
    /// <summary>
    /// The user's account ID.
    /// </summary>
    public required string AccountId { get; init; }

    /// <summary>
    /// Tag ID.
    /// </summary>
    public required string TagId { get; init; }
}
