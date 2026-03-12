using Microsoft.Extensions.Logging;
using Mystira.Core.Ports.Data;
using System.Threading;

namespace Mystira.Core.UseCases.Avatars;

/// <summary>
/// Use case for deleting the avatar configuration file
/// </summary>
public class DeleteAvatarConfigurationUseCase
{
    private readonly IAvatarConfigurationFileRepository _repository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<DeleteAvatarConfigurationUseCase> _logger;

    public DeleteAvatarConfigurationUseCase(
        IAvatarConfigurationFileRepository repository,
        IUnitOfWork unitOfWork,
        ILogger<DeleteAvatarConfigurationUseCase> logger)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<bool> ExecuteAsync(CancellationToken ct = default)
    {
        var configFile = await _repository.GetAsync(ct);
        if (configFile == null)
        {
            _logger.LogWarning("Avatar configuration file not found for deletion");
            return false;
        }

        await _repository.DeleteAsync(ct);
        await _unitOfWork.SaveChangesAsync(ct);

        _logger.LogInformation("Deleted avatar configuration file");
        return true;
    }
}

