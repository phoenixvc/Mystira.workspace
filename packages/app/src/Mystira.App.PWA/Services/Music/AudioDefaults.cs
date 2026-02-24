namespace Mystira.App.PWA.Services.Music;

/// <summary>
/// Default audio parameter constants used by the audio system.
/// </summary>
public static class AudioDefaults
{
    /// <summary>
    /// Default energy level when scene intent doesn't specify one.
    /// </summary>
    public const double DefaultEnergy = 0.45;

    /// <summary>
    /// Minimum energy difference to trigger a volume update (avoids jitter).
    /// </summary>
    public const double EnergyChangeThreshold = 0.05;

    /// <summary>
    /// Volume level when music is ducked (e.g., during narration).
    /// </summary>
    public const float DuckVolume = 0.2f;
}
