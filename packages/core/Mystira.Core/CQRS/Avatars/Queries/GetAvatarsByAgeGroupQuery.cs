using Mystira.Core.Interfaces;
using Mystira.Contracts.App.Responses.Media;

namespace Mystira.Core.CQRS.Avatars.Queries;

/// <summary>
/// Query to retrieve avatars for a specific age group.
/// </summary>
public record GetAvatarsByAgeGroupQuery(string AgeGroup)
    : IQuery<AvatarConfigurationResponse?>, ICacheableQuery
{
    public string CacheKey => $"Avatars:AgeGroup:{AgeGroup}";
    public int CacheDurationSeconds => CacheDefaults.MediumSeconds;
}
