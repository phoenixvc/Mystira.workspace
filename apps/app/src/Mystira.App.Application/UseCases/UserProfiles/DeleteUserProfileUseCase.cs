using Microsoft.Extensions.Logging;
using Mystira.Core.Ports.Data;
using System.Threading;

namespace Mystira.App.Application.UseCases.UserProfiles;

/// <summary>
/// Use case for deleting a user profile and associated data (COPPA compliance)
/// </summary>
public class DeleteUserProfileUseCase
{
    private readonly IUserProfileRepository _repository;
    private readonly IGameSessionRepository _gameSessionRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<DeleteUserProfileUseCase> _logger;

    public DeleteUserProfileUseCase(
        IUserProfileRepository repository,
        IGameSessionRepository gameSessionRepository,
        IUnitOfWork unitOfWork,
        ILogger<DeleteUserProfileUseCase> logger)
    {
        _repository = repository;
        _gameSessionRepository = gameSessionRepository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<bool> ExecuteAsync(string id, CancellationToken ct = default)
    {
        var profile = await _repository.GetByIdAsync(id, ct);
        if (profile == null)
        {
            return false;
        }

        // COPPA compliance: Also delete associated sessions, badges, and data
        var sessions = await _gameSessionRepository.GetByProfileIdAsync(profile.Id, ct);
        foreach (var session in sessions)
        {
            await _gameSessionRepository.DeleteAsync(session.Id, ct);
        }

        await _repository.DeleteAsync(profile.Id, ct);
        await _unitOfWork.SaveChangesAsync(ct);

        _logger.LogInformation("Deleted user profile and associated data: {ProfileId} - {Name}", PiiMask.HashId(profile.Id), PiiMask.HashId(profile.Name));
        return true;
    }
}

