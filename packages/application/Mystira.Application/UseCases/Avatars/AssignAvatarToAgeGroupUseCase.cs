using Microsoft.Extensions.Logging;
using Mystira.Application.Ports.Data;
using Mystira.Domain.Models;

namespace Mystira.Application.UseCases.Avatars;

/// <summary>
/// Use case for assigning avatars to an age group
/// </summary>
public class AssignAvatarToAgeGroupUseCase
{
    private readonly IAvatarConfigurationFileRepository _repository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<AssignAvatarToAgeGroupUseCase> _logger;

    /// <summary>Initializes a new instance of the <see cref="AssignAvatarToAgeGroupUseCase"/> class.</summary>
    /// <param name="repository">The avatar configuration file repository.</param>
    /// <param name="unitOfWork">The unit of work.</param>
    /// <param name="logger">The logger.</param>
    public AssignAvatarToAgeGroupUseCase(
        IAvatarConfigurationFileRepository repository,
        IUnitOfWork unitOfWork,
        ILogger<AssignAvatarToAgeGroupUseCase> logger)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    /// <summary>Assigns a list of avatar media IDs to the specified age group.</summary>
    /// <param name="ageGroup">The age group identifier.</param>
    /// <param name="mediaIds">The list of avatar media identifiers.</param>
    /// <returns>The updated avatar configuration file.</returns>
    public async Task<AvatarConfigurationFile> ExecuteAsync(string ageGroup, List<string> mediaIds)
    {
        if (string.IsNullOrWhiteSpace(ageGroup))
        {
            throw new ArgumentException("Age group cannot be null or empty", nameof(ageGroup));
        }

        if (mediaIds == null)
        {
            throw new ArgumentNullException(nameof(mediaIds));
        }

        var configFile = await _repository.GetAsync() ?? new AvatarConfigurationFile
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

        var result = await _repository.AddOrUpdateAsync(configFile);
        await _unitOfWork.SaveChangesAsync();

        _logger.LogInformation("Assigned {Count} avatars to age group {AgeGroup}", mediaIds.Count, ageGroup);
        return result;
    }
}

