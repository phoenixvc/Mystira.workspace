using Microsoft.Extensions.Logging;
using Mystira.App.Application.Mappers;
using Mystira.App.Application.Ports.Data;
using Mystira.Contracts.App.Requests.GameSessions;
using Mystira.Domain.Models;
using Mystira.Domain.Enums;
using Mystira.Domain.ValueObjects;

namespace Mystira.App.Application.UseCases.GameSessions;

/// <summary>
/// Use case for creating (starting) a new game session.
/// Contains the full business logic previously in StartGameSessionCommandHandler:
///   - Validates inputs and scenario existence
///   - Reuses existing active sessions (idempotency)
///   - Cleans up duplicate/zombie sessions
///   - Maps character assignments and initializes compass tracking
/// </summary>
public class CreateGameSessionUseCase : ICreateGameSessionUseCase
{
    private readonly IGameSessionRepository _repository;
    private readonly IScenarioRepository _scenarioRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<CreateGameSessionUseCase> _logger;

    public CreateGameSessionUseCase(
        IGameSessionRepository repository,
        IScenarioRepository scenarioRepository,
        IUnitOfWork unitOfWork,
        ILogger<CreateGameSessionUseCase> logger)
    {
        _repository = repository;
        _scenarioRepository = scenarioRepository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<UseCaseResult<GameSession>> ExecuteAsync(StartGameSessionRequest request, CancellationToken ct = default)
    {
        if (string.IsNullOrEmpty(request.ScenarioId))
            return UseCaseResult<GameSession>.Failure("ScenarioId is required");
        if (string.IsNullOrEmpty(request.AccountId))
            return UseCaseResult<GameSession>.Failure("AccountId is required");
        if (string.IsNullOrEmpty(request.ProfileId))
            return UseCaseResult<GameSession>.Failure("ProfileId is required");

        var hasPlayers = request.PlayerNames != null && request.PlayerNames.Any();
        var hasAssignments = request.CharacterAssignments != null && request.CharacterAssignments.Any();
        if (!hasPlayers && !hasAssignments)
            return UseCaseResult<GameSession>.Failure("At least one player or character assignment is required");

        // --- Idempotency: reuse existing active session if one exists ---
        var existingSession = await TryReuseExistingSessionAsync(request, ct);
        if (existingSession != null)
            return UseCaseResult<GameSession>.Success(existingSession);

        // --- Create new session ---
        var scenarioEntity = await _scenarioRepository.GetByIdAsync(request.ScenarioId, ct);
        if (scenarioEntity == null)
        {
            _logger.LogWarning(
                "Scenario {ScenarioId} was not found when starting a game session",
                request.ScenarioId);
            return UseCaseResult<GameSession>.Failure($"Scenario '{request.ScenarioId}' not found");
        }

        var targetAgeGroup = AgeGroup.Parse(request.TargetAgeGroup);
        if (targetAgeGroup == null)
        {
            _logger.LogWarning(
                "TargetAgeGroup '{TargetAgeGroup}' could not be parsed, defaulting to middle_childhood",
                request.TargetAgeGroup);
            targetAgeGroup = AgeGroup.MiddleChildhood;
        }

        if (scenarioEntity.MinimumAge > targetAgeGroup.MinAge)
        {
            return UseCaseResult<GameSession>.Failure(
                $"Scenario minimum age ({scenarioEntity.MinimumAge}) exceeds target age group ({request.TargetAgeGroup})");
        }

        var session = new GameSession
        {
            Id = Guid.NewGuid().ToString("N"),
            ScenarioId = request.ScenarioId,
            AccountId = request.AccountId,
            ProfileId = request.ProfileId,
            PlayerNames = request.PlayerNames ?? new List<string>(),
            TargetAgeGroup = targetAgeGroup.Id,
            Status = SessionStatus.InProgress,
            StartTime = DateTime.UtcNow,
            CurrentSceneId = DetermineStartingSceneId(scenarioEntity),
            ChoiceHistory = new List<SessionChoice>(),
            EchoHistory = new List<EchoLog>(),
            Achievements = new List<SessionAchievement>(),
            CompassValues = new List<CompassTracking>(),
            SceneCount = scenarioEntity.Scenes?.Count ?? 0
        };

        if (request.CharacterAssignments != null && request.CharacterAssignments.Any())
        {
            session.CharacterAssignments = (ICollection<SessionCharacterAssignment>)request.CharacterAssignments
                .Select(GameSessionMapper.ToDomain).ToList();

            if (session.PlayerNames == null || !session.PlayerNames.Any())
            {
                session.PlayerNames = DerivePlayerNames(session.CharacterAssignments.ToList());
            }
        }

        InitializeCompassTracking(session, scenarioEntity);

        await _repository.AddAsync(session, ct);
        await _unitOfWork.SaveChangesAsync(ct);

        _logger.LogInformation(
            "Started game session {SessionId} for scenario {ScenarioId}, account {AccountId}",
            session.Id, request.ScenarioId, PiiMask.HashId(request.AccountId));

        return UseCaseResult<GameSession>.Success(session);
    }

    /// <summary>
    /// Looks for existing active sessions matching the request. If found, returns the most
    /// recent one after cleaning up duplicates. Returns null when no active session exists.
    /// </summary>
    private async Task<GameSession?> TryReuseExistingSessionAsync(
        StartGameSessionRequest request, CancellationToken ct)
    {
        var existingActiveSessions = (await _repository
                .GetActiveSessionsByScenarioAndAccountAsync(request.ScenarioId, request.AccountId, ct))
            .Where(s => string.Equals(s.ProfileId, request.ProfileId, StringComparison.OrdinalIgnoreCase))
            .OrderByDescending(s => s.StartTime)
            .ToList();

        if (!existingActiveSessions.Any())
            return null;

        var primary = existingActiveSessions.First();
        var duplicates = existingActiveSessions.Skip(1).ToList();

        if (duplicates.Any())
        {
            _logger.LogWarning(
                "Found {Count} duplicate active sessions for ScenarioId={ScenarioId}, AccountId={AccountId}, ProfileId={ProfileId}. Cleaning up.",
                duplicates.Count, request.ScenarioId, PiiMask.HashId(request.AccountId), PiiMask.HashId(request.ProfileId));

            foreach (var duplicate in duplicates)
            {
                if (string.IsNullOrWhiteSpace(duplicate.CurrentSceneId) && duplicate.ChoiceHistory.Count == 0)
                {
                    await _repository.DeleteAsync(duplicate.Id, ct);
                }
                else
                {
                    duplicate.Abandon();
                    await _repository.UpdateAsync(duplicate, ct);
                }
            }

            await _unitOfWork.SaveChangesAsync(ct);
        }

        var updated = HydrateExistingSession(primary, request);

        if (string.IsNullOrWhiteSpace(primary.CurrentSceneId))
        {
            var scenario = await _scenarioRepository.GetByIdAsync(request.ScenarioId, ct);
            if (scenario != null)
            {
                primary.CurrentSceneId = DetermineStartingSceneId(scenario);
                primary.SceneCount = scenario.Scenes?.Count ?? primary.SceneCount;
                updated = true;
            }
        }

        if (updated)
        {
            await _repository.UpdateAsync(primary, ct);
            await _unitOfWork.SaveChangesAsync(ct);
        }

        _logger.LogInformation(
            "Reusing existing active game session {SessionId} for ScenarioId={ScenarioId}, AccountId={AccountId}, ProfileId={ProfileId}",
            primary.Id, request.ScenarioId, PiiMask.HashId(request.AccountId), PiiMask.HashId(request.ProfileId));

        return primary;
    }

    /// <summary>
    /// Hydrates missing assignments/names on an existing session from the current request.
    /// Returns true if any fields were updated.
    /// </summary>
    private static bool HydrateExistingSession(GameSession session, StartGameSessionRequest request)
    {
        var updated = false;

        if ((session.CharacterAssignments == null || session.CharacterAssignments.Count == 0)
            && request.CharacterAssignments != null
            && request.CharacterAssignments.Any())
        {
            session.CharacterAssignments = (ICollection<SessionCharacterAssignment>)request.CharacterAssignments
                .Select(GameSessionMapper.ToDomain).ToList();
            updated = true;
        }

        if ((session.PlayerNames == null || session.PlayerNames.Count == 0)
            && request.PlayerNames != null
            && request.PlayerNames.Any())
        {
            session.PlayerNames = request.PlayerNames.ToList();
            updated = true;
        }

        if ((session.PlayerNames == null || session.PlayerNames.Count == 0)
            && session.CharacterAssignments != null
            && session.CharacterAssignments.Any())
        {
            session.PlayerNames = DerivePlayerNames(session.CharacterAssignments.ToList());
            updated = true;
        }

        return updated;
    }

    private static List<string> DerivePlayerNames(List<SessionCharacterAssignment> assignments)
    {
        return assignments
            .Where(ca => !ca.IsUnused && ca.PlayerAssignment != null)
            .Select(ca => ca.PlayerAssignment!.ProfileName ?? ca.PlayerAssignment!.GuestName ?? "Player")
            .ToList();
    }

    private static void InitializeCompassTracking(GameSession session, Scenario? scenario)
    {
        if (scenario?.CoreAxes == null)
            return;

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
    }

    internal static string DetermineStartingSceneId(Scenario scenario)
    {
        if (scenario.Scenes == null || scenario.Scenes.Count == 0)
            return string.Empty;

        var referenced = scenario.Scenes
            .Where(s => !string.IsNullOrWhiteSpace(s.NextSceneId))
            .Select(s => s.NextSceneId!)
            .Concat(scenario.Scenes
                .SelectMany(s => s.Branches ?? new List<Branch>())
                .Where(b => !string.IsNullOrWhiteSpace(b.NextSceneId))
                .Select(b => b.NextSceneId))
            .Where(id => !string.IsNullOrWhiteSpace(id))
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        return scenario.Scenes.FirstOrDefault(s => !referenced.Contains(s.Id))?.Id
            ?? scenario.Scenes.First().Id;
    }
}
