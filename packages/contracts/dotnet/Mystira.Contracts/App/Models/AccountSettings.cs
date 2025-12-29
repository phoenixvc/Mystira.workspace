namespace Mystira.Contracts.App.Models;

/// <summary>
/// Represents account-level settings and preferences.
/// </summary>
public record AccountSettings
{
    /// <summary>
    /// Whether to enable notifications for the account.
    /// </summary>
    public bool? NotificationsEnabled { get; set; }

    /// <summary>
    /// Whether to enable email notifications.
    /// </summary>
    public bool? EmailNotificationsEnabled { get; set; }

    /// <summary>
    /// Whether to enable push notifications.
    /// </summary>
    public bool? PushNotificationsEnabled { get; set; }

    /// <summary>
    /// The preferred language code (e.g., "en", "es", "fr").
    /// </summary>
    public string? PreferredLanguage { get; set; }

    /// <summary>
    /// The preferred theme (e.g., "light", "dark", "system").
    /// </summary>
    public string? Theme { get; set; }

    /// <summary>
    /// Whether to enable sound effects.
    /// </summary>
    public bool? SoundEffectsEnabled { get; set; }

    /// <summary>
    /// Whether to enable background music.
    /// </summary>
    public bool? MusicEnabled { get; set; }

    /// <summary>
    /// The volume level for audio (0.0 to 1.0).
    /// </summary>
    public double? AudioVolume { get; set; }

    /// <summary>
    /// Whether to enable accessibility features.
    /// </summary>
    public bool? AccessibilityModeEnabled { get; set; }

    /// <summary>
    /// Whether to enable auto-save during sessions.
    /// </summary>
    public bool? AutoSaveEnabled { get; set; }

    /// <summary>
    /// Additional custom settings as key-value pairs.
    /// </summary>
    public Dictionary<string, object>? CustomSettings { get; set; }
}
