namespace Mystira.Core.Interfaces;

/// <summary>
/// Standard cache duration constants for ICacheableQuery implementations.
/// </summary>
public static class CacheDefaults
{
    /// <summary>
    /// 1 hour - for master/reference data that rarely changes (age groups, archetypes, compass axes, etc.)
    /// </summary>
    public const int MasterDataSeconds = 3600;

    /// <summary>
    /// 10 minutes - for semi-static data (avatars, attribution, featured scenarios, etc.)
    /// </summary>
    public const int MediumSeconds = 600;

    /// <summary>
    /// 5 minutes - for data that changes more frequently (scenarios, media assets, character maps)
    /// </summary>
    public const int ShortSeconds = 300;
}
