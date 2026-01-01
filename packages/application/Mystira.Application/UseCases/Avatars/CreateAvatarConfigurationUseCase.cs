using Microsoft.Extensions.Logging;
using Mystira.Application.Ports.Data;
using Mystira.Domain.Models;

namespace Mystira.Application.UseCases.Avatars;

/// <summary>
/// Use case for creating a new avatar configuration file
/// </summary>
public class CreateAvatarConfigurationUseCase
{
    private readonly IAvatarConfigurationFileRepository _repository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<CreateAvatarConfigurationUseCase> _logger;

    /// <summary>Initializes a new instance of the <see cref="CreateAvatarConfigurationUseCase"/> class.</summary>
    /// <param name="repository">The avatar configuration file repository.</param>
    /// <param name="unitOfWork">The unit of work.</param>
    /// <param name="logger">The logger.</param>
    public CreateAvatarConfigurationUseCase(
        IAvatarConfigurationFileRepository repository,
        IUnitOfWork unitOfWork,
        ILogger<CreateAvatarConfigurationUseCase> logger)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    /// <summary>Creates a new avatar configuration file with the specified age group avatars.</summary>
    /// <param name="ageGroupAvatars">The dictionary mapping age groups to avatar media IDs.</param>
    /// <returns>The created avatar configuration file.</returns>
    public async Task<AvatarConfigurationFile> ExecuteAsync(Dictionary<string, List<string>> ageGroupAvatars)
    {
        if (ageGroupAvatars == null)
        {
            throw new ArgumentNullException(nameof(ageGroupAvatars));
        }

        // Check if configuration already exists
        var existing = await _repository.GetAsync();
        if (existing != null)
        {
            throw new InvalidOperationException("Avatar configuration file already exists. Use update instead.");
        }

        var configFile = new AvatarConfigurationFile
        {
            Id = "avatar-configuration",
            AgeGroupAvatars = ageGroupAvatars,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            Version = "1.0"
        };

        await _repository.AddOrUpdateAsync(configFile);
        await _unitOfWork.SaveChangesAsync();

        _logger.LogInformation("Created avatar configuration file with {Count} age groups", ageGroupAvatars.Count);
        return configFile;
    }
}

