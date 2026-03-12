using Microsoft.Extensions.Logging;
using Mystira.Core.Ports.Data;
using Mystira.Domain.Models;
using Mystira.Domain.Enums;
using Mystira.Domain.ValueObjects;
using Mystira.Shared.Exceptions;
using Mystira.Shared.Extensions;
using Mystira.Shared.Locking;
using System.Threading;

namespace Mystira.App.Application.UseCases.GameSessions;

/// <summary>
/// Use case for checking and awarding session achievements.
/// Uses distributed locking to prevent duplicate achievement awards.
/// </summary>
public class CheckAchievementsUseCase
{
    private readonly IGameSessionRepository _repository;
    private readonly IBadgeConfigurationRepository _badgeRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IDistributedLockService _lockService;
    private readonly ILogger<CheckAchievementsUseCase> _logger;

    public CheckAchievementsUseCase(
        IGameSessionRepository repository,
        IBadgeConfigurationRepository badgeRepository,
        IUnitOfWork unitOfWork,
        IDistributedLockService lockService,
        ILogger<CheckAchievementsUseCase> logger)
    {
        _repository = repository;
        _badgeRepository = badgeRepository;
        _unitOfWork = unitOfWork;
        _lockService = lockService;
        _logger = logger;
    }

    public async Task<List<SessionAchievement>> ExecuteAsync(string sessionId, CancellationToken ct = default)
    {
        return await _lockService.ExecuteWithLockAsync(
            $"achievements:{sessionId}",
            async token => await ExecuteInternalAsync(sessionId, token),
            expiry: TimeSpan.FromSeconds(30),
            wait: TimeSpan.FromSeconds(10),
            ct);
    }

    private async Task<List<SessionAchievement>> ExecuteInternalAsync(string sessionId, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(sessionId))
        {
            throw new ValidationException("sessionId", "sessionId is required");
        }

        var session = await _repository.GetByIdAsync(sessionId, ct);
        if (session == null)
        {
            _logger.LogWarning("Game session not found: {SessionId}", sessionId);
            return new List<SessionAchievement>();
        }

        var achievements = new List<SessionAchievement>();

        // Fetch all badge configurations to check thresholds dynamically
        var badgeConfigs = await _badgeRepository.GetAllAsync(ct);

        // Check compass threshold achievements
        foreach (var compassTracking in session.CompassValues)
        {
            // Find badge configuration for this axis
            var axisBadge = badgeConfigs.FirstOrDefault(b => string.Equals(b.AxisId, compassTracking.Axis, StringComparison.OrdinalIgnoreCase));

            // Use configured threshold or fallback to 3 if not found
            var threshold = axisBadge?.Threshold ?? 3;

            if (Math.Abs(compassTracking.CurrentValue) >= threshold)
            {
                var achievementId = $"{session.Id}_{compassTracking.Axis}_threshold";
                if (!session.Achievements.Any(a => a.Id == achievementId))
                {
                    achievements.Add(new SessionAchievement
                    {
                        Id = achievementId,
                        Title = axisBadge?.Name ?? $"{compassTracking.Axis.Replace("_", " ").ToTitleCase()} Badge",
                        Description = axisBadge?.Message ?? $"Reached {compassTracking.Axis} threshold of {threshold}",
                        IconName = !string.IsNullOrEmpty(axisBadge?.ImageId) ? axisBadge.ImageId : $"badge_{compassTracking.Axis}",
                        Type = AchievementType.CompassThreshold,
                        CompassAxis = compassTracking.Axis,
                        ThresholdValue = threshold,
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

        // Add new achievements to session and save
        if (achievements.Any())
        {
            foreach (var achievement in achievements)
            {
                session.Achievements.Add(achievement);
            }

            await _repository.UpdateAsync(session, ct);
            await _unitOfWork.SaveChangesAsync(ct);

            _logger.LogInformation("Awarded {Count} achievements to session {SessionId}", achievements.Count, sessionId);
        }

        return achievements;
    }
}

