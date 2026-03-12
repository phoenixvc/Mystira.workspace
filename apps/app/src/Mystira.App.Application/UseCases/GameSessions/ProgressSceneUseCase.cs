using Microsoft.Extensions.Logging;
using Mystira.Core.Ports.Data;
using Mystira.App.Domain.Exceptions;
using Mystira.Contracts.App.Requests.GameSessions;
using Mystira.Domain.Models;
using Mystira.Domain.Enums;
using Mystira.Domain.ValueObjects;
using Mystira.Shared.Locking;
using System.Threading;

namespace Mystira.App.Application.UseCases.GameSessions;

/// <summary>
/// Use case for progressing to a specific scene in a game session.
/// Uses distributed locking to prevent concurrent modifications to the same session.
/// </summary>
public class ProgressSceneUseCase
{
    private readonly IGameSessionRepository _repository;
    private readonly IScenarioRepository _scenarioRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IDistributedLockService _lockService;
    private readonly ILogger<ProgressSceneUseCase> _logger;

    public ProgressSceneUseCase(
        IGameSessionRepository repository,
        IScenarioRepository scenarioRepository,
        IUnitOfWork unitOfWork,
        IDistributedLockService lockService,
        ILogger<ProgressSceneUseCase> logger)
    {
        _repository = repository;
        _scenarioRepository = scenarioRepository;
        _unitOfWork = unitOfWork;
        _lockService = lockService;
        _logger = logger;
    }

    public async Task<GameSession?> ExecuteAsync(ProgressSceneRequest request, CancellationToken ct = default)
    {
        return await _lockService.ExecuteWithLockAsync(
            $"session:{request.SessionId}",
            async token => await ExecuteInternalAsync(request, token),
            expiry: TimeSpan.FromSeconds(30),
            wait: TimeSpan.FromSeconds(10),
            ct);
    }

    private async Task<GameSession?> ExecuteInternalAsync(ProgressSceneRequest request, CancellationToken ct)
    {
        var session = await _repository.GetByIdAsync(request.SessionId, ct);
        if (session == null)
        {
            return null;
        }

        if (session.Status != SessionStatus.InProgress && session.Status != SessionStatus.Paused)
        {
            throw new BusinessRuleException("SessionNotInProgress", $"Cannot progress scene in session with status {session.Status}");
        }

        var scenario = await _scenarioRepository.GetByIdAsync(session.ScenarioId, ct);
        if (scenario == null)
        {
            throw new NotFoundException("Scenario", session.ScenarioId ?? "unknown");
        }

        var targetScene = scenario.Scenes.FirstOrDefault(s => s.Id == request.SceneId);
        if (targetScene == null)
        {
            throw new ValidationException("sceneId", $"Scene {request.SceneId} not found in scenario");
        }

        // Update session state
        session.CurrentSceneId = request.SceneId;
        session.ElapsedTime = DateTime.UtcNow - session.StartTime;

        // Resume if paused
        if (session.Status == SessionStatus.Paused)
        {
            session.Status = SessionStatus.InProgress;
            session.IsPaused = false;
            session.PausedAt = null;
        }

        await _repository.UpdateAsync(session, ct);
        await _unitOfWork.SaveChangesAsync(ct);

        _logger.LogInformation("Progressed session {SessionId} to scene {SceneId}",
            session.Id, request.SceneId);

        return session;
    }
}

