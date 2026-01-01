using Microsoft.Extensions.Logging;
using Mystira.Application.Ports.Data;
using Mystira.Contracts.App.Responses.Media;

namespace Mystira.Application.CQRS.Avatars.Queries;

/// <summary>
/// Wolverine handler for retrieving avatars for a specific age group.
/// Returns empty list if age group not found.
/// </summary>
public static class GetAvatarsByAgeGroupQueryHandler
{
    /// <summary>
    /// Handles the GetAvatarsByAgeGroupQuery.
    /// </summary>
    /// <param name="query">The query to handle.</param>
    /// <param name="repository">The avatar configuration file repository.</param>
    /// <param name="logger">The logger instance.</param>
    /// <param name="ct">The cancellation token.</param>
    /// <returns>The avatar configuration response for the specified age group.</returns>
    public static async Task<AvatarConfigurationResponse?> Handle(
        GetAvatarsByAgeGroupQuery query,
        IAvatarConfigurationFileRepository repository,
        ILogger<GetAvatarsByAgeGroupQuery> logger,
        CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(query.AgeGroup))
        {
            logger.LogWarning("Age group is required");
            return null;
        }

        logger.LogInformation("Retrieving avatars for age group: {AgeGroup}", query.AgeGroup);

        var configFile = await repository.GetAsync();

        if (configFile == null || !configFile.AgeGroupAvatars.TryGetValue(query.AgeGroup, out var avatars))
        {
            logger.LogInformation("No avatars found for age group: {AgeGroup}", query.AgeGroup);
            return new AvatarConfigurationResponse
            {
                AgeGroup = query.AgeGroup,
                AvatarMediaIds = new List<string>()
            };
        }

        logger.LogInformation("Found {Count} avatars for age group: {AgeGroup}",
            avatars.Count, query.AgeGroup);

        return new AvatarConfigurationResponse
        {
            AgeGroup = query.AgeGroup,
            AvatarMediaIds = avatars
        };
    }
}
