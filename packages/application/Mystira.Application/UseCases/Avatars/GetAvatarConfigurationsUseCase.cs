using Microsoft.Extensions.Logging;
using Mystira.Application.Ports.Data;
using Mystira.Contracts.App.Responses.Media;
using Mystira.Domain.Models;

namespace Mystira.Application.UseCases.Avatars;

/// <summary>
/// Use case for retrieving all avatar configurations
/// </summary>
public class GetAvatarConfigurationsUseCase
{
    private readonly IAvatarConfigurationFileRepository _repository;
    private readonly ILogger<GetAvatarConfigurationsUseCase> _logger;

    /// <summary>Initializes a new instance of the <see cref="GetAvatarConfigurationsUseCase"/> class.</summary>
    /// <param name="repository">The avatar configuration file repository.</param>
    /// <param name="logger">The logger.</param>
    public GetAvatarConfigurationsUseCase(
        IAvatarConfigurationFileRepository repository,
        ILogger<GetAvatarConfigurationsUseCase> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <summary>Retrieves all avatar configurations for all age groups.</summary>
    /// <returns>The avatar response containing avatar configurations for all age groups.</returns>
    public async Task<AvatarResponse> ExecuteAsync()
    {
        var configFile = await _repository.GetAsync();

        var response = new AvatarResponse
        {
            AgeGroupAvatars = configFile?.AgeGroupAvatars ?? new Dictionary<string, List<string>>()
        };

        // Ensure all age groups are present
        var allAgeGroups = AgeGroup.All.Select(a => a.Id).ToList();
        foreach (var ageGroup in allAgeGroups)
        {
            response.AgeGroupAvatars.TryAdd(ageGroup, new List<string>());
        }

        _logger.LogInformation("Retrieved avatar configurations for {Count} age groups", response.AgeGroupAvatars.Count);
        return response;
    }
}

