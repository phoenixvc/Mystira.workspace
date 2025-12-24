namespace Mystira.Shared.Caching;

/// <summary>
/// Configuration options for distributed caching.
/// </summary>
public class CacheOptions
{
    /// <summary>
    /// Configuration section name in appsettings.json.
    /// </summary>
    public const string SectionName = "Cache";

    /// <summary>
    /// Redis connection string. If null, falls back to in-memory cache.
    /// </summary>
    public string? RedisConnectionString { get; set; }

    /// <summary>
    /// Instance name prefix for Redis keys.
    /// </summary>
    public string InstanceName { get; set; } = "mystira:";

    /// <summary>
    /// Default cache duration in minutes.
    /// </summary>
    public int DefaultExpirationMinutes { get; set; } = 30;

    /// <summary>
    /// Short cache duration for frequently changing data.
    /// </summary>
    public int ShortExpirationMinutes { get; set; } = 5;

    /// <summary>
    /// Long cache duration for rarely changing data.
    /// </summary>
    public int LongExpirationMinutes { get; set; } = 120;

    /// <summary>
    /// Whether to use sliding expiration (reset on access).
    /// </summary>
    public bool UseSlidingExpiration { get; set; } = true;

    /// <summary>
    /// Whether caching is enabled. Can be disabled for testing.
    /// </summary>
    public bool Enabled { get; set; } = true;
}
