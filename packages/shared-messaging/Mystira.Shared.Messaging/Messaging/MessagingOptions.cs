namespace Mystira.Shared.Messaging;

/// <summary>
/// Configuration options for Wolverine messaging.
/// </summary>
public class MessagingOptions
{
    /// <summary>
    /// Configuration section name in appsettings.json.
    /// </summary>
    public const string SectionName = "Messaging";

    /// <summary>
    /// Azure Service Bus connection string for distributed messaging.
    /// If null, uses in-memory transport (development mode).
    /// </summary>
    public string? ServiceBusConnectionString { get; set; }

    /// <summary>
    /// Whether to auto-provision queues and topics on Azure Service Bus.
    /// Should be true for development, false for production.
    /// </summary>
    public bool AutoProvision { get; set; } = true;

    /// <summary>
    /// Durability mode for message handling.
    /// </summary>
    public DurabilityMode DurabilityMode { get; set; } = DurabilityMode.Balanced;

    /// <summary>
    /// Whether to use the transactional outbox for reliable messaging.
    /// </summary>
    public bool UseTransactionalOutbox { get; set; } = true;

    /// <summary>
    /// Whether messaging is enabled. Can be disabled for testing.
    /// </summary>
    public bool Enabled { get; set; } = true;

    /// <summary>
    /// Service name for topic/queue naming.
    /// </summary>
    public string ServiceName { get; set; } = "mystira";

    /// <summary>
    /// Maximum retry attempts for message processing.
    /// </summary>
    public int MaxRetries { get; set; } = 3;

    /// <summary>
    /// Initial retry delay in seconds.
    /// </summary>
    public int InitialRetryDelaySeconds { get; set; } = 5;
}

/// <summary>
/// Durability mode for Wolverine message handling.
/// </summary>
public enum DurabilityMode
{
    /// <summary>
    /// Solo mode - no durability, all messages processed in-memory.
    /// Best for development and testing.
    /// </summary>
    Solo,

    /// <summary>
    /// Balanced mode - moderate durability with reasonable performance.
    /// Best for most production scenarios.
    /// </summary>
    Balanced,

    /// <summary>
    /// MediatorOnly mode - only in-process messaging, no external transport.
    /// </summary>
    MediatorOnly,

    /// <summary>
    /// Serverless mode - optimized for serverless environments.
    /// </summary>
    Serverless
}
