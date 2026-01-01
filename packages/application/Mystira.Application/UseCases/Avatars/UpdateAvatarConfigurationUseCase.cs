using Microsoft.Extensions.Logging;
using Mystira.Application.Ports.Data;
using Mystira.Domain.Models;

namespace Mystira.Application.UseCases.Avatars;

/// <summary>
/// Use case for updating the avatar configuration file
/// </summary>
public class UpdateAvatarConfigurationUseCase
{
    private readonly IAvatarConfigurationFileRepository _repository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<UpdateAvatarConfigurationUseCase> _logger;

    /// <summary>Initializes a new instance of the <see cref="UpdateAvatarConfigurationUseCase"/> class.</summary>
    /// <param name="repository">The avatar configuration file repository.</param>
    /// <param name="unitOfWork">The unit of work.</param>
    /// <param name="logger">The logger.</param>
    public UpdateAvatarConfigurationUseCase(
        IAvatarConfigurationFileRepository repository,
        IUnitOfWork unitOfWork,
        ILogger<UpdateAvatarConfigurationUseCase> logger)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    /// <summary>Updates the avatar configuration file with the specified age group avatars.</summary>
    /// <param name="ageGroupAvatars">The dictionary mapping age groups to avatar media IDs.</param>
    /// <returns>The updated avatar configuration file.</returns>
    public async Task<AvatarConfigurationFile> ExecuteAsync(Dictionary<string, List<string>> ageGroupAvatars)
    {
        if (ageGroupAvatars == null)
        {
            throw new ArgumentNullException(nameof(ageGroupAvatars));
        }

        var configFile = await _repository.GetAsync() ?? new AvatarConfigurationFile
        {
            Id = "avatar-configuration",
            CreatedAt = DateTime.UtcNow,
            Version = "1.0"
        };

        configFile.AgeGroupAvatars = ageGroupAvatars;
        configFile.UpdatedAt = DateTime.UtcNow;

        var result = await _repository.AddOrUpdateAsync(configFile);
        await _unitOfWork.SaveChangesAsync();

        _logger.LogInformation("Updated avatar configuration file with {Count} age groups", ageGroupAvatars.Count);
        return result;
    }
}

