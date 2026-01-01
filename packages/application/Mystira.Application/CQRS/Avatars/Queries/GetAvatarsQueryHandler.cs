using Microsoft.Extensions.Logging;
using Mystira.Application.Ports.Data;
using Mystira.Contracts.App.Responses.Media;
using Mystira.Domain.ValueObjects;

namespace Mystira.Application.CQRS.Avatars.Queries;

/// <summary>
/// Wolverine handler for retrieving all avatar configurations.
/// Returns avatars grouped by age group, ensuring all age groups are initialized.
/// </summary>
public static class GetAvatarsQueryHandler
{
    public static async Task<AvatarResponse> Handle(
        GetAvatarsQuery query,
        IAvatarConfigurationFileRepository repository,
        ILogger<GetAvatarsQuery> logger,
        CancellationToken ct)
    {
        logger.LogInformation("Retrieving all avatar configurations");

        var configFile = await repository.GetAsync();

        var response = new AvatarResponse
        {
            AgeGroupAvatars = configFile?.AgeGroupAvatars ?? new Dictionary<string, List<string>>()
        };

        // Ensure all age groups are present
        foreach (var ageGroup in AgeGroup.All)
        {
            response.AgeGroupAvatars.TryAdd(ageGroup.Id, new List<string>());
        }

        logger.LogInformation("Retrieved avatars for {Count} age groups", response.AgeGroupAvatars.Count);
        return response;
    }
}
