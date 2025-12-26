namespace Mystira.Shared.Messaging.Events;

/// <summary>
/// Published when a new device is registered.
/// </summary>
public sealed record DeviceRegistered : IntegrationEventBase
{
    /// <summary>
    /// The device ID.
    /// </summary>
    public required string DeviceId { get; init; }

    /// <summary>
    /// The user's account ID.
    /// </summary>
    public required string AccountId { get; init; }

    /// <summary>
    /// Device type (mobile, tablet, desktop, tv).
    /// </summary>
    public required string DeviceType { get; init; }

    /// <summary>
    /// Platform (ios, android, web, windows, macos).
    /// </summary>
    public required string Platform { get; init; }

    /// <summary>
    /// Device name for display.
    /// </summary>
    public string? DeviceName { get; init; }

    /// <summary>
    /// App version.
    /// </summary>
    public required string AppVersion { get; init; }
}

/// <summary>
/// Published when a device is removed/deauthorized.
/// </summary>
public sealed record DeviceRemoved : IntegrationEventBase
{
    /// <summary>
    /// The device ID.
    /// </summary>
    public required string DeviceId { get; init; }

    /// <summary>
    /// The user's account ID.
    /// </summary>
    public required string AccountId { get; init; }

    /// <summary>
    /// Removal reason (user_initiated, security, inactive).
    /// </summary>
    public required string Reason { get; init; }

    /// <summary>
    /// Whether all sessions on device were invalidated.
    /// </summary>
    public bool SessionsInvalidated { get; init; } = true;
}

/// <summary>
/// Published when session state is synced across devices.
/// </summary>
public sealed record SessionSynced : IntegrationEventBase
{
    /// <summary>
    /// The session ID.
    /// </summary>
    public required string SessionId { get; init; }

    /// <summary>
    /// The user's account ID.
    /// </summary>
    public required string AccountId { get; init; }

    /// <summary>
    /// Source device ID.
    /// </summary>
    public required string SourceDeviceId { get; init; }

    /// <summary>
    /// Target device ID.
    /// </summary>
    public required string TargetDeviceId { get; init; }

    /// <summary>
    /// Sync type (full, incremental).
    /// </summary>
    public required string SyncType { get; init; }

    /// <summary>
    /// Data size in bytes.
    /// </summary>
    public int DataSizeBytes { get; init; }
}

/// <summary>
/// Published when a sync conflict is detected.
/// </summary>
public sealed record SyncConflictDetected : IntegrationEventBase
{
    /// <summary>
    /// The session ID.
    /// </summary>
    public required string SessionId { get; init; }

    /// <summary>
    /// The user's account ID.
    /// </summary>
    public required string AccountId { get; init; }

    /// <summary>
    /// Device IDs involved in conflict.
    /// </summary>
    public required string[] ConflictingDeviceIds { get; init; }

    /// <summary>
    /// Conflict type (progress, choice, state).
    /// </summary>
    public required string ConflictType { get; init; }

    /// <summary>
    /// How it was resolved (latest_wins, user_choice, merge).
    /// </summary>
    public string? Resolution { get; init; }
}

/// <summary>
/// Published when user initiates handoff to another device.
/// </summary>
public sealed record DeviceHandoffStarted : IntegrationEventBase
{
    /// <summary>
    /// Handoff ID.
    /// </summary>
    public required string HandoffId { get; init; }

    /// <summary>
    /// The user's account ID.
    /// </summary>
    public required string AccountId { get; init; }

    /// <summary>
    /// Source device ID.
    /// </summary>
    public required string SourceDeviceId { get; init; }

    /// <summary>
    /// Target device ID.
    /// </summary>
    public required string TargetDeviceId { get; init; }

    /// <summary>
    /// Session being handed off.
    /// </summary>
    public required string SessionId { get; init; }
}

/// <summary>
/// Published when device handoff completes.
/// </summary>
public sealed record DeviceHandoffCompleted : IntegrationEventBase
{
    /// <summary>
    /// Handoff ID.
    /// </summary>
    public required string HandoffId { get; init; }

    /// <summary>
    /// The user's account ID.
    /// </summary>
    public required string AccountId { get; init; }

    /// <summary>
    /// Duration of handoff in milliseconds.
    /// </summary>
    public required int DurationMs { get; init; }

    /// <summary>
    /// Whether handoff was successful.
    /// </summary>
    public required bool Success { get; init; }

    /// <summary>
    /// Error if failed.
    /// </summary>
    public string? ErrorMessage { get; init; }
}

/// <summary>
/// Published when push notification settings are updated.
/// </summary>
public sealed record PushSettingsUpdated : IntegrationEventBase
{
    /// <summary>
    /// The device ID.
    /// </summary>
    public required string DeviceId { get; init; }

    /// <summary>
    /// The user's account ID.
    /// </summary>
    public required string AccountId { get; init; }

    /// <summary>
    /// Whether push is enabled.
    /// </summary>
    public required bool PushEnabled { get; init; }

    /// <summary>
    /// Categories enabled (messages, achievements, updates).
    /// </summary>
    public string[]? EnabledCategories { get; init; }
}
