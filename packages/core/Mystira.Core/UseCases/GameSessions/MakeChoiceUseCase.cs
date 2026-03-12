using Microsoft.Extensions.Logging;
using Mystira.Core.Ports.Data;
using Mystira.Shared.Exceptions;
using Mystira.Contracts.App.Requests.GameSessions;
using Mystira.Domain.Models;
using Mystira.Domain.Enums;
using Mystira.Domain.ValueObjects;
using Mystira.Shared.Locking;
using System.Threading;

namespace Mystira.Core.UseCases.GameSessions;

/// <summary>
/// Use case for making a choice in a game session.
/// Uses distributed locking to prevent concurrent modifications to the same session.
/// </summary>
public class MakeChoiceUseCase
{
    private readonly IGameSessionRepository _repository;
    private readonly IScenarioRepository _scenarioRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IDistributedLockService _lockService;
    private readonly ILogger<MakeChoiceUseCase> _logger;

    public MakeChoiceUseCase(
        IGameSessionRepository repository,
        IScenarioRepository scenarioRepository,
        IUnitOfWork unitOfWork,
        IDistributedLockService lockService,
        ILogger<MakeChoiceUseCase> logger)
    {
        _repository = repository;
        _scenarioRepository = scenarioRepository;
        _unitOfWork = unitOfWork;
        _lockService = lockService;
        _logger = logger;
    }

    public async Task<GameSession?> ExecuteAsync(MakeChoiceRequest request, CancellationToken ct = default)
    {
        return await _lockService.ExecuteWithLockAsync(
            $"session:{request.SessionId}",
            async token => await ExecuteInternalAsync(request, token),
            expiry: TimeSpan.FromSeconds(30),
            wait: TimeSpan.FromSeconds(10),
            ct);
    }

    private async Task<GameSession?> ExecuteInternalAsync(MakeChoiceRequest request, CancellationToken ct)
    {
        var session = await _repository.GetByIdAsync(request.SessionId, ct);
        if (session == null)
        {
            return null;
        }

        if (session.Status != SessionStatus.InProgress)
        {
            throw new BusinessRuleException("SessionNotInProgress", $"Cannot make choice in session with status {session.Status}");
        }

        var scenario = await _scenarioRepository.GetByIdAsync(session.ScenarioId, ct);
        if (scenario == null)
        {
            throw new NotFoundException("Scenario", session.ScenarioId ?? "unknown");
        }

        var currentScene = scenario.Scenes.FirstOrDefault(s => s.Id == request.SceneId);
        if (currentScene == null)
        {
            throw new ValidationException("input", "Scene not found in scenario");
        }

        var branch = currentScene.Branches.FirstOrDefault(b => b.Choice == request.ChoiceText);
        if (branch == null)
        {
            throw new ValidationException("input", "Choice not found in scene");
        }

        // Resolve the player who made the decision for choice scenes using ActiveCharacter assignment
        string? playerId = null;
        if (currentScene.Type == SceneType.Choice && !string.IsNullOrWhiteSpace(currentScene.ActiveCharacter))
        {
            var assignment = session.CharacterAssignments.FirstOrDefault(a =>
                string.Equals(a.CharacterId, currentScene.ActiveCharacter, StringComparison.OrdinalIgnoreCase));

            var assignedProfileId = assignment?.PlayerAssignment?.ProfileId;
            if (!string.IsNullOrWhiteSpace(assignedProfileId))
            {
                playerId = assignedProfileId;
            }
            else
            {
                _logger.LogError(
                    "ActiveCharacter '{ActiveCharacter}' for scene '{SceneId}' could not resolve a player assignment in session {SessionId}.",
                    currentScene.ActiveCharacter, currentScene.Id, session.Id);
            }
        }

        // Fallbacks: explicit request PlayerId, then session owner ProfileId
        playerId = !string.IsNullOrWhiteSpace(playerId)
            ? playerId
            : (!string.IsNullOrWhiteSpace(request.PlayerId) ? request.PlayerId : session.ProfileId);

        var compassAxis = request.CompassAxis ?? branch.CompassChange?.AxisId;
        var compassDelta = request.CompassDelta ?? (double?)branch.CompassChange?.Delta;
        var compassDirection = request.CompassDirection;

        var sessionChoice = new SessionChoice
        {
            SceneId = request.SceneId,
            SceneTitle = currentScene.Title,
            ChoiceText = request.ChoiceText,
            NextScene = request.NextSceneId,
            PlayerId = playerId ?? string.Empty,
            CompassAxis = compassAxis,
            CompassDirection = compassDirection,
            CompassDelta = compassDelta ?? 0.0,
            ChosenAt = DateTime.UtcNow,
            EchoGenerated = branch.EchoLog != null,
            CompassChange = !string.IsNullOrWhiteSpace(compassAxis) && compassDelta.HasValue
                ? new CompassChange { AxisId = compassAxis, Delta = (int)compassDelta.Value }
                : null
        };

        session.ChoiceHistory.Add(sessionChoice);

        if (branch.EchoLog != null)
        {
            session.EchoHistory.Add(new EchoLog
            {
                EchoTypeId = branch.EchoLog.EchoTypeId,
                Content = branch.EchoLog.Content,
                Strength = branch.EchoLog.Strength,
                Timestamp = DateTime.UtcNow
            });
        }

        session.CompassValues ??= new List<CompassTracking>();
        foreach (var axis in scenario.CoreAxes)
        {
            if (!session.CompassValues.Any(cv => cv.Axis == axis))
            {
                session.CompassValues.Add(new CompassTracking
                {
                    Axis = axis,
                    CurrentValue = 0.0,
                    StartingValue = 0,
                    History = new List<CompassChangeRecord>(),
                    LastUpdated = DateTime.UtcNow
                });
            }
        }

        session.CurrentSceneId = request.NextSceneId;
        session.ElapsedTime = DateTime.UtcNow - (session.StartTime ?? DateTime.UtcNow);

        var nextScene = scenario.Scenes.FirstOrDefault(s => s.Id == request.NextSceneId);
        if (nextScene == null || (!nextScene.Branches.Any() && string.IsNullOrEmpty(nextScene.NextSceneId)))
        {
            session.Status = SessionStatus.Completed;
            session.EndTime = DateTime.UtcNow;
        }

        session.RecalculateCompassProgressFromHistory();

        await _repository.UpdateAsync(session, ct);
        await _unitOfWork.SaveChangesAsync(ct);

        _logger.LogInformation(
            "Choice made in session {SessionId}: {ChoiceText} -> {NextScene} (PlayerId={PlayerId})",
            session.Id,
            request.ChoiceText,
            request.NextSceneId,
            PiiMask.HashId(playerId));

        return session;
    }
}
