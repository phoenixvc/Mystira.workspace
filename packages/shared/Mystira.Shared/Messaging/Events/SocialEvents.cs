namespace Mystira.Shared.Messaging.Events;

/// <summary>
/// Published when a user rates a scenario.
/// </summary>
public sealed record ScenarioRated : IntegrationEventBase
{
    /// <summary>
    /// The user's account ID.
    /// </summary>
    public required string AccountId { get; init; }

    /// <summary>
    /// The scenario ID.
    /// </summary>
    public required string ScenarioId { get; init; }

    /// <summary>
    /// Rating value (1-5).
    /// </summary>
    public required int Rating { get; init; }

    /// <summary>
    /// Previous rating if updating.
    /// </summary>
    public int? PreviousRating { get; init; }
}

/// <summary>
/// Published when a user reviews a scenario.
/// </summary>
public sealed record ScenarioReviewed : IntegrationEventBase
{
    /// <summary>
    /// The review ID.
    /// </summary>
    public required string ReviewId { get; init; }

    /// <summary>
    /// The user's account ID.
    /// </summary>
    public required string AccountId { get; init; }

    /// <summary>
    /// The scenario ID.
    /// </summary>
    public required string ScenarioId { get; init; }

    /// <summary>
    /// Rating value (1-5).
    /// </summary>
    public required int Rating { get; init; }

    /// <summary>
    /// Whether review has text content.
    /// </summary>
    public bool HasTextContent { get; init; }

    /// <summary>
    /// Review text length.
    /// </summary>
    public int TextLength { get; init; }

    /// <summary>
    /// Whether user completed the scenario before reviewing.
    /// </summary>
    public bool HasCompletedScenario { get; init; }
}

/// <summary>
/// Published when a user shares a scenario.
/// </summary>
public sealed record ScenarioShared : IntegrationEventBase
{
    /// <summary>
    /// The user's account ID.
    /// </summary>
    public required string AccountId { get; init; }

    /// <summary>
    /// The scenario ID.
    /// </summary>
    public required string ScenarioId { get; init; }

    /// <summary>
    /// Share platform (twitter, facebook, link, email).
    /// </summary>
    public required string Platform { get; init; }

    /// <summary>
    /// Share ID for tracking.
    /// </summary>
    public required string ShareId { get; init; }
}

/// <summary>
/// Published when a user follows another user.
/// </summary>
public sealed record UserFollowed : IntegrationEventBase
{
    /// <summary>
    /// The follower's account ID.
    /// </summary>
    public required string FollowerAccountId { get; init; }

    /// <summary>
    /// The followed user's account ID.
    /// </summary>
    public required string FollowedAccountId { get; init; }

    /// <summary>
    /// Whether this is a mutual follow.
    /// </summary>
    public bool IsMutual { get; init; }
}

/// <summary>
/// Published when a user unfollows another user.
/// </summary>
public sealed record UserUnfollowed : IntegrationEventBase
{
    /// <summary>
    /// The unfollower's account ID.
    /// </summary>
    public required string FollowerAccountId { get; init; }

    /// <summary>
    /// The unfollowed user's account ID.
    /// </summary>
    public required string UnfollowedAccountId { get; init; }
}

/// <summary>
/// Published when a comment is posted.
/// </summary>
public sealed record CommentPosted : IntegrationEventBase
{
    /// <summary>
    /// The comment ID.
    /// </summary>
    public required string CommentId { get; init; }

    /// <summary>
    /// The author's account ID.
    /// </summary>
    public required string AccountId { get; init; }

    /// <summary>
    /// Target type (scenario, review, profile).
    /// </summary>
    public required string TargetType { get; init; }

    /// <summary>
    /// Target ID.
    /// </summary>
    public required string TargetId { get; init; }

    /// <summary>
    /// Parent comment ID if this is a reply.
    /// </summary>
    public string? ParentCommentId { get; init; }

    /// <summary>
    /// Comment text length.
    /// </summary>
    public int TextLength { get; init; }
}

/// <summary>
/// Published when a comment is deleted.
/// </summary>
public sealed record CommentDeleted : IntegrationEventBase
{
    /// <summary>
    /// The comment ID.
    /// </summary>
    public required string CommentId { get; init; }

    /// <summary>
    /// The author's account ID.
    /// </summary>
    public required string AccountId { get; init; }

    /// <summary>
    /// Who deleted it (author, moderator, system).
    /// </summary>
    public required string DeletedBy { get; init; }

    /// <summary>
    /// Deletion reason if not self-deleted.
    /// </summary>
    public string? Reason { get; init; }
}

/// <summary>
/// Published when a user likes content.
/// </summary>
public sealed record ContentLiked : IntegrationEventBase
{
    /// <summary>
    /// The user's account ID.
    /// </summary>
    public required string AccountId { get; init; }

    /// <summary>
    /// Content type (scenario, review, comment).
    /// </summary>
    public required string ContentType { get; init; }

    /// <summary>
    /// Content ID.
    /// </summary>
    public required string ContentId { get; init; }

    /// <summary>
    /// Content owner's account ID.
    /// </summary>
    public required string ContentOwnerId { get; init; }
}

/// <summary>
/// Published when a user unlikes content.
/// </summary>
public sealed record ContentUnliked : IntegrationEventBase
{
    /// <summary>
    /// The user's account ID.
    /// </summary>
    public required string AccountId { get; init; }

    /// <summary>
    /// Content type.
    /// </summary>
    public required string ContentType { get; init; }

    /// <summary>
    /// Content ID.
    /// </summary>
    public required string ContentId { get; init; }
}
