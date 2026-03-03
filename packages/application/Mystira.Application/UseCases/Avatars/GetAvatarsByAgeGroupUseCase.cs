using Microsoft.Extensions.Logging;
using Mystira.Application.Ports.Data;
using Mystira.Contracts.App.Responses.Media;

namespace Mystira.Application.UseCases.Avatars;

/// <summary>
/// Use case for retrieving avatars for a specific age group
/// </summary>
public class GetAvatarsByAgeGroupUseCase
{
    private readonly IAvatarConfigurationFileRepository _repository;
    private readonly ILogger<GetAvatarsByAgeGroupUseCase> _logger;

    /// <summary>Initializes a new instance of the <see cref="GetAvatarsByAgeGroupUseCase"/> class.</summary>
    /// <param name="repository">The avatar configuration file repository.</param>
    /// <param name="logger">The logger.</param>
    public GetAvatarsByAgeGroupUseCase(
        IAvatarConfigurationFileRepository repository,
        ILogger<GetAvatarsByAgeGroupUseCase> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <summary>Retrieves avatars for the specified age group.</summary>
    /// <param name="ageGroup">The age group identifier.</param>
    /// <returns>The avatar configuration response containing avatar media IDs for the age group.</returns>
    public async Task<AvatarConfigurationResponse> ExecuteAsync(string ageGroup)
    {
        if (string.IsNullOrWhiteSpace(ageGroup))
        {
            throw new ArgumentException("Age group cannot be null or empty", nameof(ageGroup));
        }

        var configFile = await _repository.GetAsync();

        if (configFile == null || !configFile.AgeGroupAvatars.TryGetValue(ageGroup, out var avatars))
        {
            _logger.LogDebug("No avatars found for age group: {AgeGroup}", ageGroup);
            return new AvatarConfigurationResponse
            {
                AgeGroup = ageGroup,
                AvatarMediaIds = new List<string>()
            };
        }

        _logger.LogInformation("Retrieved {Count} avatars for age group {AgeGroup}", avatars.Count, ageGroup);
        return new AvatarConfigurationResponse
        {
            AgeGroup = ageGroup,
            AvatarMediaIds = avatars
        };
    }
}

