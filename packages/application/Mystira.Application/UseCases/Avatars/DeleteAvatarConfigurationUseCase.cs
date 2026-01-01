using Microsoft.Extensions.Logging;
using Mystira.Application.Ports.Data;

namespace Mystira.Application.UseCases.Avatars;

/// <summary>
/// Use case for deleting the avatar configuration file
/// </summary>
public class DeleteAvatarConfigurationUseCase
{
    private readonly IAvatarConfigurationFileRepository _repository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<DeleteAvatarConfigurationUseCase> _logger;

    /// <summary>Initializes a new instance of the <see cref="DeleteAvatarConfigurationUseCase"/> class.</summary>
    /// <param name="repository">The avatar configuration file repository.</param>
    /// <param name="unitOfWork">The unit of work.</param>
    /// <param name="logger">The logger.</param>
    public DeleteAvatarConfigurationUseCase(
        IAvatarConfigurationFileRepository repository,
        IUnitOfWork unitOfWork,
        ILogger<DeleteAvatarConfigurationUseCase> logger)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    /// <summary>Deletes the avatar configuration file.</summary>
    /// <returns>True if the configuration file was deleted successfully; otherwise, false.</returns>
    public async Task<bool> ExecuteAsync()
    {
        var configFile = await _repository.GetAsync();
        if (configFile == null)
        {
            _logger.LogWarning("Avatar configuration file not found for deletion");
            return false;
        }

        await _repository.DeleteAsync();
        await _unitOfWork.SaveChangesAsync();

        _logger.LogInformation("Deleted avatar configuration file");
        return true;
    }
}

