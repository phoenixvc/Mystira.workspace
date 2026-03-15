using Microsoft.Extensions.Logging;
using Mystira.Shared.Messaging.Events;
using StackExchange.Redis;

namespace Mystira.Shared.Messaging.Handlers;

/// <summary>
/// Handles account-related events to invalidate cached account data.
/// </summary>
public class AccountEventHandler
{
    private readonly IConnectionMultiplexer _redis;
    private readonly ILogger<AccountEventHandler> _logger;

    public AccountEventHandler(
        IConnectionMultiplexer redis,
        ILogger<AccountEventHandler> logger)
    {
        _redis = redis;
        _logger = logger;
    }

    public async Task HandleAsync(UserLoggedIn loggedIn, CancellationToken ct)
    {
        await InvalidateCacheAsync($"account:{loggedIn.AccountId}");
        await InvalidateCacheByPatternAsync("accounts:");
    }

    public async Task HandleAsync(PasswordChanged passwordChanged, CancellationToken ct)
    {
        await InvalidateCacheAsync($"account:{passwordChanged.AccountId}");
    }

    private async Task InvalidateCacheAsync(string key)
    {
        var db = _redis.GetDatabase();
        await db.KeyDeleteAsync(key);
        _logger.LogDebug("Invalidated cache key: {CacheKey}", key);
    }

    private async Task InvalidateCacheByPatternAsync(string pattern)
    {
        var server = _redis.GetServer(_redis.GetEndPoints().First());
        var keys = server.Keys(pattern: $"*{pattern}*").ToArray();
        
        if (keys.Length > 0)
        {
            var db = _redis.GetDatabase();
            foreach (var key in keys)
            {
                await db.KeyDeleteAsync(key);
            }
            _logger.LogDebug("Invalidated {Count} cache keys with pattern: {Pattern}", keys.Length, pattern);
        }
    }
}

/// <summary>
/// Handles game session events to invalidate cached session data.
/// </summary>
public class GameSessionEventHandler
{
    private readonly IConnectionMultiplexer _redis;
    private readonly ILogger<GameSessionEventHandler> _logger;

    public GameSessionEventHandler(
        IConnectionMultiplexer redis,
        ILogger<GameSessionEventHandler> logger)
    {
        _redis = redis;
        _logger = logger;
    }

    public async Task HandleAsync(SessionStarted sessionStarted, CancellationToken ct)
    {
        await InvalidateCacheAsync($"session:{sessionStarted.SessionId}");
        await InvalidateCacheByPatternAsync("sessions:");
    }

    public async Task HandleAsync(SessionCompleted sessionCompleted, CancellationToken ct)
    {
        await InvalidateCacheAsync($"session:{sessionCompleted.SessionId}");
        await InvalidateCacheByPatternAsync("sessions:");
    }

    public async Task HandleAsync(ChoiceMade choiceMade, CancellationToken ct)
    {
        await InvalidateCacheAsync($"session:{choiceMade.SessionId}");
        await InvalidateCacheAsync($"progress:{choiceMade.AccountId}");
    }

    public async Task HandleAsync(ChapterStarted chapterStarted, CancellationToken ct)
    {
        await InvalidateCacheAsync($"session:{chapterStarted.SessionId}");
        await InvalidateCacheByPatternAsync("chapters:");
    }

    public async Task HandleAsync(ChapterCompleted chapterCompleted, CancellationToken ct)
    {
        await InvalidateCacheAsync($"session:{chapterCompleted.SessionId}");
        await InvalidateCacheByPatternAsync("chapters:");
    }

    public async Task HandleAsync(CheckpointReached checkpointReached, CancellationToken ct)
    {
        await InvalidateCacheAsync($"session:{checkpointReached.SessionId}");
        await InvalidateCacheByPatternAsync("checkpoints:");
    }

    public async Task HandleAsync(ProgressSaved progressSaved, CancellationToken ct)
    {
        await InvalidateCacheAsync($"session:{progressSaved.SessionId}");
        await InvalidateCacheByPatternAsync("progress:");
    }

    private async Task InvalidateCacheAsync(string key)
    {
        var db = _redis.GetDatabase();
        await db.KeyDeleteAsync(key);
        _logger.LogDebug("Invalidated cache key: {CacheKey}", key);
    }

    private async Task InvalidateCacheByPatternAsync(string pattern)
    {
        var server = _redis.GetServer(_redis.GetEndPoints().First());
        var keys = server.Keys(pattern: $"*{pattern}*").ToArray();
        
        if (keys.Length > 0)
        {
            var db = _redis.GetDatabase();
            foreach (var key in keys)
            {
                await db.KeyDeleteAsync(key);
            }
            _logger.LogDebug("Invalidated {Count} cache keys with pattern: {Pattern}", keys.Length, pattern);
        }
    }
}

/// <summary>
/// Handles scenario/content events to invalidate cached content data.
/// </summary>
public class ContentEventHandler
{
    private readonly IConnectionMultiplexer _redis;
    private readonly ILogger<ContentEventHandler> _logger;

    public ContentEventHandler(
        IConnectionMultiplexer redis,
        ILogger<ContentEventHandler> logger)
    {
        _redis = redis;
        _logger = logger;
    }

    public async Task HandleAsync(ScenarioCreated scenarioCreated, CancellationToken ct)
    {
        await InvalidateCacheAsync($"scenario:{scenarioCreated.ScenarioId}");
        await InvalidateCacheByPatternAsync("scenarios:");
    }

    public async Task HandleAsync(ScenarioUpdated scenarioUpdated, CancellationToken ct)
    {
        await InvalidateCacheAsync($"scenario:{scenarioUpdated.ScenarioId}");
        await InvalidateCacheByPatternAsync("scenarios:");
    }

    public async Task HandleAsync(ScenarioPublished scenarioPublished, CancellationToken ct)
    {
        await InvalidateCacheAsync($"scenario:{scenarioPublished.ScenarioId}");
        await InvalidateCacheByPatternAsync("scenarios:");
    }

    public async Task HandleAsync(ScenarioUnpublished scenarioUnpublished, CancellationToken ct)
    {
        await InvalidateCacheAsync($"scenario:{scenarioUnpublished.ScenarioId}");
        await InvalidateCacheByPatternAsync("scenarios:");
    }

    public async Task HandleAsync(MediaUploaded mediaUploaded, CancellationToken ct)
    {
        await InvalidateCacheAsync($"media:{mediaUploaded.MediaId}");
        await InvalidateCacheByPatternAsync("media:");
    }

    private async Task InvalidateCacheAsync(string key)
    {
        var db = _redis.GetDatabase();
        await db.KeyDeleteAsync(key);
        _logger.LogDebug("Invalidated cache key: {CacheKey}", key);
    }

    private async Task InvalidateCacheByPatternAsync(string pattern)
    {
        var server = _redis.GetServer(_redis.GetEndPoints().First());
        var keys = server.Keys(pattern: $"*{pattern}*").ToArray();
        
        if (keys.Length > 0)
        {
            var db = _redis.GetDatabase();
            foreach (var key in keys)
            {
                await db.KeyDeleteAsync(key);
            }
            _logger.LogDebug("Invalidated {Count} cache keys with pattern: {Pattern}", keys.Length, pattern);
        }
    }
}

/// <summary>
/// Handles badge/achievement events to invalidate cached badge data.
/// </summary>
public class BadgeEventHandler
{
    private readonly IConnectionMultiplexer _redis;
    private readonly ILogger<BadgeEventHandler> _logger;

    public BadgeEventHandler(
        IConnectionMultiplexer redis,
        ILogger<BadgeEventHandler> logger)
    {
        _redis = redis;
        _logger = logger;
    }

    public async Task HandleAsync(BadgeEarned badgeEarned, CancellationToken ct)
    {
        await InvalidateCacheAsync($"badge:{badgeEarned.BadgeId}");
        await InvalidateCacheAsync($"userbadges:{badgeEarned.AccountId}");
        await InvalidateCacheByPatternAsync("badges:");
    }

    public async Task HandleAsync(AchievementUnlocked achievementUnlocked, CancellationToken ct)
    {
        await InvalidateCacheAsync($"achievement:{achievementUnlocked.AchievementId}");
        await InvalidateCacheByPatternAsync("achievements:");
    }

    public async Task HandleAsync(XPEarned xpEarned, CancellationToken ct)
    {
        await InvalidateCacheAsync($"xp:{xpEarned.AccountId}");
        await InvalidateCacheByPatternAsync("leaderboard:");
    }

    public async Task HandleAsync(LevelUp levelUp, CancellationToken ct)
    {
        await InvalidateCacheAsync($"level:{levelUp.AccountId}");
        await InvalidateCacheByPatternAsync("leaderboard:");
    }

    public async Task HandleAsync(StreakUpdated streakUpdated, CancellationToken ct)
    {
        await InvalidateCacheAsync($"streak:{streakUpdated.AccountId}");
    }

    private async Task InvalidateCacheAsync(string key)
    {
        var db = _redis.GetDatabase();
        await db.KeyDeleteAsync(key);
        _logger.LogDebug("Invalidated cache key: {CacheKey}", key);
    }

    private async Task InvalidateCacheByPatternAsync(string pattern)
    {
        var server = _redis.GetServer(_redis.GetEndPoints().First());
        var keys = server.Keys(pattern: $"*{pattern}*").ToArray();
        
        if (keys.Length > 0)
        {
            var db = _redis.GetDatabase();
            foreach (var key in keys)
            {
                await db.KeyDeleteAsync(key);
            }
            _logger.LogDebug("Invalidated {Count} cache keys with pattern: {Pattern}", keys.Length, pattern);
        }
    }
}

/// <summary>
/// Handles cache invalidation events to clear Redis cache entries.
/// </summary>
public class CacheInvalidationHandler
{
    private readonly IConnectionMultiplexer _redis;
    private readonly ILogger<CacheInvalidationHandler> _logger;

    public CacheInvalidationHandler(
        IConnectionMultiplexer redis,
        ILogger<CacheInvalidationHandler> logger)
    {
        _redis = redis;
        _logger = logger;
    }

    public async Task HandleAsync(
        CacheInvalidateEvent cacheEvent,
        CancellationToken ct)
    {
        try
        {
            var db = _redis.GetDatabase();
            
            if (!string.IsNullOrEmpty(cacheEvent.CacheKey))
            {
                await db.KeyDeleteAsync(cacheEvent.CacheKey);
                _logger.LogDebug("Invalidated cache key: {CacheKey}", cacheEvent.CacheKey);
            }
            
            if (!string.IsNullOrEmpty(cacheEvent.CacheKeyPrefix))
            {
                await InvalidateByPatternAsync(db, cacheEvent.CacheKeyPrefix);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to invalidate cache for event: {EventType}", cacheEvent.GetType().Name);
        }
    }

    private async Task InvalidateByPatternAsync(IDatabase db, string pattern)
    {
        var server = _redis.GetServer(_redis.GetEndPoints().First());
        var keys = server.Keys(pattern: $"*{pattern}*").ToArray();
        
        foreach (var key in keys)
        {
            await db.KeyDeleteAsync(key);
        }
        
        _logger.LogDebug("Invalidated {Count} cache keys with pattern: {Pattern}", keys.Length, pattern);
    }
}

/// <summary>
/// Event for explicit cache invalidation requests.
/// </summary>
public sealed record CacheInvalidateEvent : IntegrationEventBase
{
    public required string CacheKey { get; init; }
    public string? CacheKeyPrefix { get; init; }
}
