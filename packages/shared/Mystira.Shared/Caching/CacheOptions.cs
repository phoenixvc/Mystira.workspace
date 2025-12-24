using System.ComponentModel.DataAnnotations;
using Microsoft.Extensions.Options;

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
    [Range(1, int.MaxValue, ErrorMessage = "DefaultExpirationMinutes must be positive")]
    public int DefaultExpirationMinutes { get; set; } = 60;

    /// <summary>
    /// Short cache duration for frequently changing data.
    /// </summary>
    [Range(1, int.MaxValue, ErrorMessage = "ShortExpirationMinutes must be positive")]
    public int ShortExpirationMinutes { get; set; } = 5;

    /// <summary>
    /// Long cache duration for rarely changing data.
    /// </summary>
    [Range(1, int.MaxValue, ErrorMessage = "LongExpirationMinutes must be positive")]
    public int LongExpirationMinutes { get; set; } = 120;

    /// <summary>
    /// Whether to use sliding expiration (reset on access).
    /// </summary>
    public bool UseSlidingExpiration { get; set; } = false;

    /// <summary>
    /// Whether caching is enabled. Can be disabled for testing.
    /// </summary>
    public bool Enabled { get; set; } = true;
}

/// <summary>
/// Validator for CacheOptions that enforces logical constraints.
/// </summary>
public class CacheOptionsValidator : IValidateOptions<CacheOptions>
{
    public ValidateOptionsResult Validate(string? name, CacheOptions options)
    {
        var failures = new List<string>();

        if (options.DefaultExpirationMinutes <= 0)
        {
            failures.Add("DefaultExpirationMinutes must be positive");
        }

        if (options.ShortExpirationMinutes <= 0)
        {
            failures.Add("ShortExpirationMinutes must be positive");
        }

        if (options.LongExpirationMinutes <= 0)
        {
            failures.Add("LongExpirationMinutes must be positive");
        }

        // Validate logical ordering: short < default < long
        if (options.ShortExpirationMinutes >= options.DefaultExpirationMinutes)
        {
            failures.Add("ShortExpirationMinutes must be less than DefaultExpirationMinutes");
        }

        if (options.DefaultExpirationMinutes >= options.LongExpirationMinutes)
        {
            failures.Add("DefaultExpirationMinutes must be less than LongExpirationMinutes");
        }

        return failures.Count > 0
            ? ValidateOptionsResult.Fail(failures)
            : ValidateOptionsResult.Success;
    }
}
