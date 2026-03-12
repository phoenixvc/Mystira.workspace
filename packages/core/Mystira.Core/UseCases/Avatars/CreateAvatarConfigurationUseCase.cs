using Microsoft.Extensions.Logging;
using Mystira.Core.Ports.Data;
using Mystira.Shared.Exceptions;
using Mystira.Domain.Models;
using Mystira.Domain.Enums;
using Mystira.Domain.ValueObjects;
using System.Threading;

namespace Mystira.Core.UseCases.Avatars;

/// <summary>
/// Use case for creating a new avatar configuration file
/// </summary>
public class CreateAvatarConfigurationUseCase
{
    private readonly IAvatarConfigurationFileRepository _repository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<CreateAvatarConfigurationUseCase> _logger;

    public CreateAvatarConfigurationUseCase(
        IAvatarConfigurationFileRepository repository,
        IUnitOfWork unitOfWork,
        ILogger<CreateAvatarConfigurationUseCase> logger)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<AvatarConfigurationFile> ExecuteAsync(Dictionary<string, List<string>> ageGroupAvatars, CancellationToken ct = default)
    {
        if (ageGroupAvatars == null)
        {
            throw new ValidationException("ageGroupAvatars", "ageGroupAvatars is required");
        }

        // Check if configuration already exists
        var existing = await _repository.GetAsync(ct);
        if (existing != null)
        {
            throw new ConflictException("Avatar configuration file already exists. Use update instead.");
        }

        var configFile = new AvatarConfigurationFile
        {
            Id = "avatar-configuration",
            AgeGroupAvatars = ageGroupAvatars,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            Version = "1.0"
        };

        await _repository.AddOrUpdateAsync(configFile, ct);
        await _unitOfWork.SaveChangesAsync(ct);

        _logger.LogInformation("Created avatar configuration file with {Count} age groups", ageGroupAvatars.Count);
        return configFile;
    }
}

