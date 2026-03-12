using Microsoft.Extensions.Logging;
using Mystira.Core.Ports.Data;
using Mystira.Contracts.App.Responses.Media;
using Mystira.Domain.Models;
using Mystira.Domain.Enums;
using Mystira.Domain.ValueObjects;
using System.Threading;

namespace Mystira.Core.UseCases.Avatars;

/// <summary>
/// Use case for retrieving all avatar configurations
/// </summary>
public class GetAvatarConfigurationsUseCase
{
    private readonly IAvatarConfigurationFileRepository _repository;
    private readonly ILogger<GetAvatarConfigurationsUseCase> _logger;

    public GetAvatarConfigurationsUseCase(
        IAvatarConfigurationFileRepository repository,
        ILogger<GetAvatarConfigurationsUseCase> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    public async Task<AvatarResponse> ExecuteAsync(CancellationToken ct = default)
    {
        var configFile = await _repository.GetAsync(ct);

        var response = new AvatarResponse
        {
            AgeGroupAvatars = configFile?.AgeGroupAvatars ?? new Dictionary<string, List<string>>()
        };

        // Ensure all age groups are present
        foreach (var ageGroup in AgeGroupConstants.GetAll())
        {
            response.AgeGroupAvatars.TryAdd(ageGroup, new List<string>());
        }

        _logger.LogInformation("Retrieved avatar configurations for {Count} age groups", response.AgeGroupAvatars.Count);
        return response;
    }
}

