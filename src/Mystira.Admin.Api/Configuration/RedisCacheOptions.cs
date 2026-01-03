namespace Mystira.Admin.Api.Configuration;

/// <summary>
/// Configuration options for Redis caching in Admin API.
/// </summary>
public class RedisCacheOptions
{
    public const string SectionName = "Redis";

    /// <summary>
    /// Redis instance name prefix for cache keys.
    /// Prevents key collisions across environments.
    /// Example: "mystira-admin-dev:" or "mystira-admin-prod:"
    /// </summary>
    public string InstanceName { get; set; } = "mystira-admin:";

    /// <summary>
    /// Cache duration for content entities (scenarios, characters, badges).
    /// </summary>
    public int ContentCacheMinutes { get; set; } = 30;

    /// <summary>
    /// Cache duration for user data lookups (accounts, profiles).
    /// Shorter duration since user data changes more frequently.
    /// </summary>
    public int UserCacheMinutes { get; set; } = 5;

    /// <summary>
    /// Cache duration for master data (compass axes, archetypes, etc.).
    /// Longer duration since master data rarely changes.
    /// </summary>
    public int MasterDataCacheMinutes { get; set; } = 60;

    /// <summary>
    /// Enable distributed cache invalidation via Redis pub/sub.
    /// Required for multi-instance deployments.
    /// </summary>
    public bool EnableDistributedInvalidation { get; set; } = true;

    /// <summary>
    /// Redis pub/sub channel for cache invalidation messages.
    /// </summary>
    public string InvalidationChannel { get; set; } = "mystira:cache:invalidation";
}
