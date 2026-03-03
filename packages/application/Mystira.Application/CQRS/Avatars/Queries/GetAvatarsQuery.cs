using Mystira.Application.Interfaces;
using Mystira.Contracts.App.Responses.Media;

namespace Mystira.Application.CQRS.Avatars.Queries;

/// <summary>
/// Query to retrieve all avatar configurations grouped by age group.
/// </summary>
public record GetAvatarsQuery : IQuery<AvatarResponse>, ICacheableQuery
{
    /// <summary>
    /// Gets the cache key for storing this query result.
    /// </summary>
    public string CacheKey => "AllAvatars";

    /// <summary>
    /// Gets the cache duration in seconds.
    /// </summary>
    public int CacheDurationSeconds => 600; // 10 minutes
}
