using Mystira.Application.Interfaces;
using Mystira.Contracts.App.Responses.Media;

namespace Mystira.Application.CQRS.Avatars.Queries;

/// <summary>
/// Query to retrieve avatars for a specific age group.
/// </summary>
/// <param name="AgeGroup">The age group to retrieve avatars for.</param>
public record GetAvatarsByAgeGroupQuery(string AgeGroup)
    : IQuery<AvatarConfigurationResponse?>, ICacheableQuery
{
    /// <summary>
    /// Gets the cache key for storing this query result.
    /// </summary>
    public string CacheKey => $"Avatars:AgeGroup:{AgeGroup}";

    /// <summary>
    /// Gets the cache duration in seconds.
    /// </summary>
    public int CacheDurationSeconds => 600; // 10 minutes
}
