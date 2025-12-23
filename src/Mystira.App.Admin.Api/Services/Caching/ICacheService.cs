namespace Mystira.App.Admin.Api.Services.Caching;

/// <summary>
/// Cache service interface for distributed caching operations.
/// Supports both in-memory (development) and Redis (production) backends.
/// </summary>
public interface ICacheService
{
    /// <summary>
    /// Get a cached item by key.
    /// </summary>
    /// <typeparam name="T">Type of the cached item.</typeparam>
    /// <param name="key">Cache key.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The cached item or null if not found.</returns>
    Task<T?> GetAsync<T>(string key, CancellationToken cancellationToken = default) where T : class;

    /// <summary>
    /// Set a cached item with optional expiration.
    /// </summary>
    /// <typeparam name="T">Type of the item to cache.</typeparam>
    /// <param name="key">Cache key.</param>
    /// <param name="value">Value to cache.</param>
    /// <param name="expiration">Optional expiration time. Uses default if not specified.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    Task SetAsync<T>(string key, T value, TimeSpan? expiration = null, CancellationToken cancellationToken = default) where T : class;

    /// <summary>
    /// Remove a cached item by key.
    /// </summary>
    /// <param name="key">Cache key.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    Task RemoveAsync(string key, CancellationToken cancellationToken = default);

    /// <summary>
    /// Remove all cached items matching a pattern.
    /// </summary>
    /// <param name="pattern">Key pattern (e.g., "scenarios:*").</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    Task RemoveByPatternAsync(string pattern, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get or set a cached item using a factory function.
    /// </summary>
    /// <typeparam name="T">Type of the cached item.</typeparam>
    /// <param name="key">Cache key.</param>
    /// <param name="factory">Factory function to create the value if not cached.</param>
    /// <param name="expiration">Optional expiration time.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The cached or newly created item.</returns>
    Task<T?> GetOrSetAsync<T>(
        string key,
        Func<CancellationToken, Task<T?>> factory,
        TimeSpan? expiration = null,
        CancellationToken cancellationToken = default) where T : class;
}

/// <summary>
/// Cache key builder for consistent key generation.
/// </summary>
public static class CacheKeys
{
    public const string Prefix = "mystira:admin:";

    // Scenarios
    public static string Scenario(string id) => $"{Prefix}scenario:{id}";
    public static string ScenariosList => $"{Prefix}scenarios:list";
    public static string ScenariosPattern => $"{Prefix}scenario:*";

    // Characters
    public static string CharacterMap(string id) => $"{Prefix}character:{id}";
    public static string CharacterMapsList => $"{Prefix}characters:list";
    public static string CharactersPattern => $"{Prefix}character:*";

    // Bundles
    public static string Bundle(string id) => $"{Prefix}bundle:{id}";
    public static string BundlesList => $"{Prefix}bundles:list";
    public static string BundlesPattern => $"{Prefix}bundle:*";

    // Badges
    public static string Badge(string id) => $"{Prefix}badge:{id}";
    public static string BadgesList => $"{Prefix}badges:list";
    public static string BadgesPattern => $"{Prefix}badge:*";

    // Master Data (longer cache)
    public static string CompassAxes => $"{Prefix}master:compass-axes";
    public static string Archetypes => $"{Prefix}master:archetypes";
    public static string EchoTypes => $"{Prefix}master:echo-types";
    public static string FantasyThemes => $"{Prefix}master:fantasy-themes";
    public static string MasterDataPattern => $"{Prefix}master:*";

    // User Data (shorter cache, read-only)
    public static string Account(string id) => $"{Prefix}account:{id}";
    public static string Profile(string id) => $"{Prefix}profile:{id}";
    public static string AccountsPattern => $"{Prefix}account:*";
    public static string ProfilesPattern => $"{Prefix}profile:*";
}
