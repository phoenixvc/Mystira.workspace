using Microsoft.Extensions.Logging;
using Mystira.App.Application.Ports.Data;
using Mystira.App.Domain.Models;
using Mystira.Shared.Exceptions;
using System.Threading;

namespace Mystira.App.Application.UseCases.Avatars;

/// <summary>
/// Use case for assigning avatars to an age group
/// </summary>
public class AssignAvatarToAgeGroupUseCase
{
    private readonly IAvatarConfigurationFileRepository _repository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<AssignAvatarToAgeGroupUseCase> _logger;

    public AssignAvatarToAgeGroupUseCase(
        IAvatarConfigurationFileRepository repository,
        IUnitOfWork unitOfWork,
        ILogger<AssignAvatarToAgeGroupUseCase> logger)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<AvatarConfigurationFile> ExecuteAsync(string ageGroup, List<string> mediaIds, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(ageGroup))
        {
            throw new ValidationException("ageGroup", "ageGroup is required");
        }

        if (mediaIds == null)
        {
            throw new ValidationException("mediaIds", "mediaIds is required");
        }

        var configFile = await _repository.GetAsync(ct) ?? new AvatarConfigurationFile
        {
            Id = "avatar-configuration",
            CreatedAt = DateTime.UtcNow,
            Version = "1.0"
        };

        if (configFile.AgeGroupAvatars == null)
        {
            configFile.AgeGroupAvatars = new Dictionary<string, List<string>>();
        }

        configFile.AgeGroupAvatars[ageGroup] = mediaIds;
        configFile.UpdatedAt = DateTime.UtcNow;

        var result = await _repository.AddOrUpdateAsync(configFile, ct);
        await _unitOfWork.SaveChangesAsync(ct);

        _logger.LogInformation("Assigned {Count} avatars to age group {AgeGroup}", mediaIds.Count, ageGroup);
        return result;
    }
}

