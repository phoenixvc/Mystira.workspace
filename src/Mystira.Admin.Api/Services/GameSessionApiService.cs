using Microsoft.EntityFrameworkCore;
using Mystira.Domain.Models;
using Mystira.Domain.Enums;
using Mystira.Domain.ValueObjects;
using Mystira.Infrastructure.Data;
using ContractsGameSessionResponse = Mystira.Contracts.App.Responses.GameSessions.GameSessionResponse;
using ContractsMakeChoiceRequest = Mystira.Contracts.App.Requests.GameSessions.MakeChoiceRequest;
using ContractsSessionStatsResponse = Mystira.Contracts.App.Responses.GameSessions.SessionStatsResponse;
using ContractsStartGameSessionRequest = Mystira.Contracts.App.Requests.GameSessions.StartGameSessionRequest;

namespace Mystira.Admin.Api.Services;

public class GameSessionApiService : IGameSessionApiService
{
    private readonly MystiraAppDbContext _context;
    private readonly IScenarioApiService _scenarioService;
    private readonly ILogger<GameSessionApiService> _logger;

    public GameSessionApiService(
        MystiraAppDbContext context,
        IScenarioApiService scenarioService,
        ILogger<GameSessionApiService> logger)
    {
        _context = context;
        _scenarioService = scenarioService;
        _logger = logger;
    }

    public async Task<GameSession> StartSessionAsync(ContractsStartGameSessionRequest request)
    {
        // Validate scenario exists
        var scenario = await _scenarioService.GetScenarioByIdAsync(request.ScenarioId);
        if (scenario == null)
        {
            throw new ArgumentException($"Scenario not found: {request.ScenarioId}");
        }

        // Validate age appropriateness
        if (!IsAgeGroupCompatible(scenario.MinimumAge, request.TargetAgeGroup))
        {
            throw new ArgumentException($"Scenario minimum age ({scenario.MinimumAge}) exceeds target age group ({request.TargetAgeGroup})");
        }

        // Check for existing active sessions for this scenario and account
        var existingActiveSessions = await _context.GameSessions
            .Where(s => s.ScenarioId == request.ScenarioId &&
                       s.AccountId == request.AccountId &&
                       (s.Status == SessionStatus.InProgress || s.Status == SessionStatus.Paused))
            .ToListAsync();

        // Auto-complete any existing active sessions for this scenario
        if (existingActiveSessions.Any())
        {
            _logger.LogInformation("Found {Count} existing active session(s) for scenario {ScenarioId} and account {AccountId}. Completing them.",
                existingActiveSessions.Count, request.ScenarioId, request.AccountId);

            foreach (var existingSession in existingActiveSessions)
            {
                existingSession.Status = SessionStatus.Completed;
                existingSession.EndTime = DateTime.UtcNow;
                if (existingSession.EndTime.HasValue)
                {
                    existingSession.ElapsedTime = existingSession.EndTime.Value - existingSession.StartTime;
                }
            }

            await _context.SaveChangesAsync();
        }

        var session = new GameSession
        {
            Id = Guid.NewGuid().ToString(),
            ScenarioId = request.ScenarioId,
            AccountId = request.AccountId,
            ProfileId = request.ProfileId,
            PlayerNames = request.PlayerNames,
            Status = SessionStatus.InProgress,
            CurrentSceneId = scenario.Scenes.First().Id,
            StartTime = DateTime.UtcNow,
            TargetAgeGroupName = request.TargetAgeGroup,
            SceneCount = scenario.Scenes.Count
        };

        // Initialize compass tracking for scenario axes
        session.CompassValues = scenario.CoreAxes.Select(axis => new CompassTracking
        {
            Axis = axis,
            CurrentValue = 0.0f,
            History = new List<CompassChangeRecord>(),
            LastUpdated = DateTime.UtcNow
        }).ToList();

        _context.GameSessions.Add(session);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Started new game session: {SessionId} for Account: {AccountId}, Profile: {ProfileId}",
            session.Id, session.AccountId, session.ProfileId);
        return session;
    }

    public async Task<GameSession?> GetSessionAsync(string sessionId)
    {
        return await _context.GameSessions
            .FirstOrDefaultAsync(s => s.Id == sessionId);
    }

    public async Task<List<ContractsGameSessionResponse>> GetSessionsByAccountAsync(string accountId)
    {
        var sessions = await _context.GameSessions
            .Where(s => s.AccountId == accountId)
            .OrderByDescending(s => s.StartTime)
            .ToListAsync();

        return sessions.Select(s => MapToResponse(s)).ToList();
    }

    public async Task<List<ContractsGameSessionResponse>> GetSessionsByProfileAsync(string profileId)
    {
        var sessions = await _context.GameSessions
            .Where(s => s.ProfileId == profileId)
            .OrderByDescending(s => s.StartTime)
            .ToListAsync();

        return sessions.Select(s => MapToResponse(s)).ToList();
    }

    private static ContractsGameSessionResponse MapToResponse(GameSession s)
    {
        return new ContractsGameSessionResponse
        {
            Id = s.Id,
            ScenarioId = s.ScenarioId,
            AccountId = s.AccountId,
            ProfileId = s.ProfileId,
            PlayerNames = s.PlayerNames,
            Status = s.Status.ToString(),
            CurrentSceneId = s.CurrentSceneId,
            ChoiceCount = s.ChoiceHistory?.Count ?? 0,
            EchoCount = s.EchoHistory?.Count ?? 0,
            AchievementCount = s.Achievements?.Count ?? 0,
            StartTime = s.StartTime ?? DateTime.UtcNow,
            EndTime = s.EndTime ?? DateTime.MinValue,
            ElapsedTime = s.ElapsedTime,
            IsPaused = s.IsPaused,
            SceneCount = s.SceneCount,
            TargetAgeGroup = s.TargetAgeGroupName
        };
    }

    public async Task<GameSession?> MakeChoiceAsync(ContractsMakeChoiceRequest request)
    {
        var session = await GetSessionAsync(request.SessionId);
        if (session == null)
        {
            return null;
        }

        if (session.Status != SessionStatus.InProgress)
        {
            throw new InvalidOperationException("Cannot make choice in non-active session");
        }

        var scenario = await _scenarioService.GetScenarioByIdAsync(session.ScenarioId);
        if (scenario == null)
        {
            throw new InvalidOperationException("Scenario not found for session");
        }

        var currentScene = scenario.Scenes.FirstOrDefault(s => s.Id == request.SceneId);
        if (currentScene == null)
        {
            throw new ArgumentException("Scene not found in scenario");
        }

        var branch = currentScene.Branches.FirstOrDefault(b => b.Choice == request.ChoiceText);
        if (branch == null)
        {
            throw new ArgumentException("Choice not found in scene");
        }

        var playerId = !string.IsNullOrWhiteSpace(request.PlayerId)
            ? request.PlayerId
            : session.ProfileId;

        var compassAxis = request.CompassAxis ?? branch.CompassChange?.Axis;
        var compassDelta = request.CompassDelta ?? branch.CompassChange?.Delta;
        var compassDirection = request.CompassDirection;

        // Record the choice
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
            // CompassChange mapping skipped - branch.CompassChange is CompassChangeDto, not CompassChange
            CompassChange = null
        };

        session.ChoiceHistory.Add(sessionChoice);

        // Process echo log if present
        if (branch.EchoLog != null)
        {
            session.EchoHistory.Add(branch.EchoLog);
        }

        session.RecalculateCompassProgressFromHistory();

        // Update session state
        session.CurrentSceneId = request.NextSceneId;
        session.ElapsedTime = DateTime.UtcNow - session.StartTime;

        // Check for achievements
        var newAchievements = await CheckAchievementsAsync(session.Id);
        foreach (var achievement in newAchievements.Where(a => !session.Achievements.Any(sa => sa.Id == a.Id)))
        {
            session.Achievements.Add(achievement);
        }

        // Check if session is complete (reached end scene or no more branches)
        var nextScene = scenario.Scenes.FirstOrDefault(s => s.Id == request.NextSceneId);
        if (nextScene == null || !nextScene.Branches.Any())
        {
            session.Status = SessionStatus.Completed;
            session.EndTime = DateTime.UtcNow;
        }

        await _context.SaveChangesAsync();

        _logger.LogInformation("Choice made in session {SessionId}: {ChoiceText} -> {NextScene}",
            session.Id, request.ChoiceText, request.NextSceneId);

        return session;
    }

    public async Task<GameSession?> PauseSessionAsync(string sessionId)
    {
        var session = await GetSessionAsync(sessionId);
        if (session == null)
        {
            return null;
        }

        if (session.Status != SessionStatus.InProgress)
        {
            throw new InvalidOperationException("Can only pause sessions in progress");
        }

        session.Status = SessionStatus.Paused;
        session.IsPaused = true;
        session.PausedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        _logger.LogInformation("Paused session: {SessionId}", sessionId);
        return session;
    }

    public async Task<GameSession?> ResumeSessionAsync(string sessionId)
    {
        var session = await GetSessionAsync(sessionId);
        if (session == null)
        {
            return null;
        }

        if (session.Status != SessionStatus.Paused)
        {
            throw new InvalidOperationException("Can only resume paused sessions");
        }

        session.Status = SessionStatus.InProgress;
        session.IsPaused = false;
        session.PausedAt = null;

        await _context.SaveChangesAsync();

        _logger.LogInformation("Resumed session: {SessionId}", sessionId);
        return session;
    }

    public async Task<GameSession?> EndSessionAsync(string sessionId)
    {
        var session = await GetSessionAsync(sessionId);
        if (session == null)
        {
            return null;
        }

        session.Status = SessionStatus.Completed;
        session.EndTime = DateTime.UtcNow;
        if (session.EndTime.HasValue)
        {
            session.ElapsedTime = session.EndTime.Value - session.StartTime;
        }
        session.IsPaused = false;

        await _context.SaveChangesAsync();

        _logger.LogInformation("Ended session: {SessionId}", sessionId);
        return session;
    }

    public async Task<ContractsSessionStatsResponse?> GetSessionStatsAsync(string sessionId)
    {
        var session = await GetSessionAsync(sessionId);
        if (session == null)
        {
            return null;
        }

        var compassValues = session.CompassValues
            .ToDictionary(ct => ct.Axis, ct => ct.CurrentValue);

        var recentEchoes = session.EchoHistory
            .OrderByDescending(e => e.Timestamp)
            .Take(5)
            .Cast<object>()
            .ToList();

        var achievements = session.Achievements?.Cast<object>().ToList() ?? new List<object>();

        var endTime = session.EndTime ?? DateTime.UtcNow;
        var startTime = session.StartTime ?? DateTime.UtcNow;
        var duration = endTime.Subtract(startTime);

        return new ContractsSessionStatsResponse
        {
            CompassValues = compassValues,
            RecentEchoes = recentEchoes,
            Achievements = achievements,
            TotalChoices = session.ChoiceHistory?.Count ?? 0,
            SessionDuration = duration
        };
    }

    public async Task<List<SessionAchievement>> CheckAchievementsAsync(string sessionId)
    {
        var session = await GetSessionAsync(sessionId);
        if (session == null)
        {
            return new List<SessionAchievement>();
        }

        var achievements = new List<SessionAchievement>();
        // TODO: Replace with BadgeConfigurationApiService to get dynamic badge thresholds
        // For now, using simple threshold logic as placeholder
        var defaultThreshold = 3.0f;

        // Check compass threshold achievements
        foreach (var compassTracking in session.CompassValues)
        {
            if (Math.Abs(compassTracking.CurrentValue) >= defaultThreshold)
            {
                var achievementId = $"{session.Id}_{compassTracking.Axis}_threshold";
                if (!session.Achievements.Any(a => a.Id == achievementId))
                {
                    achievements.Add(new SessionAchievement
                    {
                        Id = achievementId,
                        Title = $"{compassTracking.Axis.Replace("_", " ").ToTitleCase()} Badge",
                        Description = $"Reached {compassTracking.Axis} threshold of {defaultThreshold}",
                        IconName = $"badge_{compassTracking.Axis}",
                        Type = AchievementType.CompassThreshold,
                        CompassAxis = compassTracking.Axis,
                        ThresholdValue = (int)defaultThreshold,
                        EarnedAt = DateTime.UtcNow
                    });
                }
            }
        }

        // Check first choice achievement
        if (session.ChoiceHistory.Count == 1)
        {
            var firstChoiceId = $"{session.Id}_first_choice";
            if (!session.Achievements.Any(a => a.Id == firstChoiceId))
            {
                achievements.Add(new SessionAchievement
                {
                    Id = firstChoiceId,
                    Title = "First Steps",
                    Description = "Made your first choice in the adventure",
                    IconName = "badge_first_choice",
                    Type = AchievementType.FirstChoice,
                    EarnedAt = DateTime.UtcNow
                });
            }
        }

        // Check session completion achievement
        if (session.Status == SessionStatus.Completed)
        {
            var completionId = $"{session.Id}_completion";
            if (!session.Achievements.Any(a => a.Id == completionId))
            {
                achievements.Add(new SessionAchievement
                {
                    Id = completionId,
                    Title = "Adventure Complete",
                    Description = "Successfully completed the adventure",
                    IconName = "badge_completion",
                    Type = AchievementType.SessionComplete,
                    EarnedAt = DateTime.UtcNow
                });
            }
        }

        return achievements;
    }

    public async Task<bool> DeleteSessionAsync(string sessionId)
    {
        var session = await GetSessionAsync(sessionId);
        if (session == null)
        {
            return false;
        }

        _context.GameSessions.Remove(session);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Deleted session: {SessionId}", sessionId);
        return true;
    }

    public async Task<GameSession?> SelectCharacterAsync(string sessionId, string characterId)
    {
        var session = await _context.GameSessions.FirstOrDefaultAsync(s => s.Id == sessionId);
        if (session == null)
        {
            return null;
        }

        session.SelectedCharacterId = characterId;
        await _context.SaveChangesAsync();

        _logger.LogInformation("Selected character {CharacterId} for session {SessionId}", characterId, sessionId);
        return session;
    }

    public async Task<List<GameSession>> GetSessionsForProfileAsync(string profileId)
    {
        try
        {
            // Game sessions can be linked to profiles in multiple ways:
            // 1. By profile ID (if the profile owns the session)
            // 2. By player names (if the profile is a player)

            var sessions = await _context.GameSessions
                .Where(s => s.ProfileId == profileId || s.PlayerNames.Contains(profileId))
                .OrderByDescending(s => s.StartTime)
                .ToListAsync();

            return sessions;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting sessions for profile {ProfileId}", profileId);
            return new List<GameSession>();
        }
    }

    private bool IsAgeGroupCompatible(int scenarioMinimumAge, string targetAgeGroup)
    {
        if (scenarioMinimumAge <= 0)
        {
            return true;
        }

        if (string.IsNullOrWhiteSpace(targetAgeGroup))
        {
            return true;
        }

        // Map known age group values to their minimum ages
        var minimumAge = targetAgeGroup.ToLowerInvariant() switch
        {
            "younger-kids" => 5,
            "older-kids" => 8,
            "teens" => 11,
            "adults" => 15,
            _ => (int?)null
        };

        if (minimumAge.HasValue)
        {
            return minimumAge.Value >= scenarioMinimumAge;
        }

        if (TryParseAgeRangeMinimum(targetAgeGroup, out var parsedMinimum))
        {
            return parsedMinimum >= scenarioMinimumAge;
        }

        return true;
    }

    private static bool TryParseAgeRangeMinimum(string value, out int minimumAge)
    {
        minimumAge = 0;
        var parts = value.Split('-', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        if (parts.Length > 0 && int.TryParse(parts[0], out var min))
        {
            minimumAge = min;
            return true;
        }

        return false;
    }

    public async Task<int> GetActiveSessionsCountAsync()
    {
        try
        {
            return await _context.GameSessions
                .CountAsync(s => s.Status == SessionStatus.InProgress || s.Status == SessionStatus.Paused);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting active sessions count");
            return 0;
        }
    }

    public async Task<GameSession?> ProgressSessionSceneAsync(string sessionId, string newSceneId)
    {
        try
        {
            var session = await GetSessionAsync(sessionId);
            if (session == null)
            {
                _logger.LogWarning("Session not found: {SessionId}", sessionId);
                return null;
            }

            if (session.Status != SessionStatus.InProgress)
            {
                _logger.LogWarning("Cannot progress scene for session {SessionId} with status {Status}",
                    sessionId, session.Status);
                return null;
            }

            // Update the current scene
            session.CurrentSceneId = newSceneId;
            session.ElapsedTime = DateTime.UtcNow - session.StartTime;

            await _context.SaveChangesAsync();

            _logger.LogInformation("Progressed session {SessionId} to scene {SceneId}", sessionId, newSceneId);
            return session;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error progressing session {SessionId} to scene {SceneId}", sessionId, newSceneId);
            return null;
        }
    }
}

// Extension method for title case conversion
public static class StringExtensions
{
    public static string ToTitleCase(this string input)
    {
        if (string.IsNullOrEmpty(input))
        {
            return input;
        }

        var words = input.Split(' ', '_', '-');
        for (int i = 0; i < words.Length; i++)
        {
            if (words[i].Length > 0)
            {
                words[i] = char.ToUpper(words[i][0]) + words[i].Substring(1).ToLower();
            }
        }
        return string.Join(" ", words);
    }
}
