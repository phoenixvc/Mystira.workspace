namespace Mystira.Shared.Messaging.Events;

/// <summary>
/// Published when user preferences are updated.
/// </summary>
public sealed record PreferencesUpdated : IntegrationEventBase
{
    /// <summary>
    /// The user's account ID.
    /// </summary>
    public required string AccountId { get; init; }

    /// <summary>
    /// Category of preference (display, privacy, notifications, gameplay).
    /// </summary>
    public required string Category { get; init; }

    /// <summary>
    /// Preference key.
    /// </summary>
    public required string Key { get; init; }

    /// <summary>
    /// New value (serialized).
    /// </summary>
    public required string NewValue { get; init; }

    /// <summary>
    /// Previous value if known.
    /// </summary>
    public string? PreviousValue { get; init; }
}

/// <summary>
/// Published when theme is changed.
/// </summary>
public sealed record ThemeChanged : IntegrationEventBase
{
    /// <summary>
    /// The user's account ID.
    /// </summary>
    public required string AccountId { get; init; }

    /// <summary>
    /// Previous theme.
    /// </summary>
    public required string FromTheme { get; init; }

    /// <summary>
    /// New theme.
    /// </summary>
    public required string ToTheme { get; init; }
}

/// <summary>
/// Published when language is changed.
/// </summary>
public sealed record LanguageChanged : IntegrationEventBase
{
    /// <summary>
    /// The user's account ID.
    /// </summary>
    public required string AccountId { get; init; }

    /// <summary>
    /// Previous language code.
    /// </summary>
    public required string FromLanguage { get; init; }

    /// <summary>
    /// New language code.
    /// </summary>
    public required string ToLanguage { get; init; }
}

/// <summary>
/// Published when accessibility settings change.
/// </summary>
public sealed record AccessibilitySettingChanged : IntegrationEventBase
{
    /// <summary>
    /// The user's account ID.
    /// </summary>
    public required string AccountId { get; init; }

    /// <summary>
    /// Setting name (high_contrast, screen_reader, font_size, etc.).
    /// </summary>
    public required string Setting { get; init; }

    /// <summary>
    /// Whether it's now enabled.
    /// </summary>
    public required bool Enabled { get; init; }

    /// <summary>
    /// Value if applicable (e.g., font size).
    /// </summary>
    public string? Value { get; init; }
}

/// <summary>
/// Published when privacy settings change.
/// </summary>
public sealed record PrivacySettingChanged : IntegrationEventBase
{
    /// <summary>
    /// The user's account ID.
    /// </summary>
    public required string AccountId { get; init; }

    /// <summary>
    /// Setting (profile_visibility, activity_status, search_indexing).
    /// </summary>
    public required string Setting { get; init; }

    /// <summary>
    /// New value (public, friends_only, private).
    /// </summary>
    public required string NewValue { get; init; }
}

/// <summary>
/// Published when notification preferences change.
/// </summary>
public sealed record NotificationPreferenceChanged : IntegrationEventBase
{
    /// <summary>
    /// The user's account ID.
    /// </summary>
    public required string AccountId { get; init; }

    /// <summary>
    /// Channel (email, push, in_app).
    /// </summary>
    public required string Channel { get; init; }

    /// <summary>
    /// Category (marketing, social, achievements, system).
    /// </summary>
    public required string Category { get; init; }

    /// <summary>
    /// Whether now enabled.
    /// </summary>
    public required bool Enabled { get; init; }
}

/// <summary>
/// Published when content preferences change (used for recommendations).
/// </summary>
public sealed record ContentPreferenceUpdated : IntegrationEventBase
{
    /// <summary>
    /// The user's account ID.
    /// </summary>
    public required string AccountId { get; init; }

    /// <summary>
    /// Preference type (genres, themes, difficulty, content_rating).
    /// </summary>
    public required string PreferenceType { get; init; }

    /// <summary>
    /// Values selected.
    /// </summary>
    public required string[] Values { get; init; }
}

/// <summary>
/// Published when parental controls are updated.
/// </summary>
public sealed record ParentalControlsUpdated : IntegrationEventBase
{
    /// <summary>
    /// The user's account ID (parent).
    /// </summary>
    public required string ParentAccountId { get; init; }

    /// <summary>
    /// The child's account ID if applicable.
    /// </summary>
    public string? ChildAccountId { get; init; }

    /// <summary>
    /// Control type (content_rating, time_limit, purchases, chat).
    /// </summary>
    public required string ControlType { get; init; }

    /// <summary>
    /// New restriction level/value.
    /// </summary>
    public required string RestrictionValue { get; init; }
}
