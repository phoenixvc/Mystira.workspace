namespace Mystira.Shared.Locking;

/// <summary>
/// Represents a distributed lock that can be acquired across multiple instances.
/// </summary>
public interface IDistributedLockHandle : IAsyncDisposable
{
    /// <summary>
    /// The unique identifier of the lock.
    /// </summary>
    string LockId { get; }

    /// <summary>
    /// The resource key that was locked.
    /// </summary>
    string Resource { get; }

    /// <summary>
    /// Whether the lock is currently held.
    /// </summary>
    bool IsAcquired { get; }

    /// <summary>
    /// When the lock was acquired.
    /// </summary>
    DateTimeOffset? AcquiredAt { get; }

    /// <summary>
    /// When the lock will expire if not released or extended.
    /// </summary>
    DateTimeOffset? ExpiresAt { get; }

    /// <summary>
    /// Extends the lock by the specified duration.
    /// </summary>
    /// <param name="extension">The duration to extend the lock by.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>True if the lock was successfully extended.</returns>
    Task<bool> ExtendAsync(TimeSpan extension, CancellationToken cancellationToken = default);

    /// <summary>
    /// Releases the lock explicitly.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    Task ReleaseAsync(CancellationToken cancellationToken = default);
}

/// <summary>
/// Service for acquiring distributed locks across multiple instances.
/// Useful for preventing concurrent modifications to shared resources.
/// </summary>
public interface IDistributedLockService
{
    /// <summary>
    /// Attempts to acquire a distributed lock on the specified resource.
    /// </summary>
    /// <param name="resource">The resource key to lock (e.g., "account:123", "order:456").</param>
    /// <param name="expiry">How long the lock should be held before auto-expiring.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A lock handle if acquired, or null if the lock could not be acquired.</returns>
    Task<IDistributedLockHandle?> TryAcquireAsync(
        string resource,
        TimeSpan expiry,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Acquires a distributed lock, waiting up to the specified time.
    /// </summary>
    /// <param name="resource">The resource key to lock.</param>
    /// <param name="expiry">How long the lock should be held before auto-expiring.</param>
    /// <param name="wait">Maximum time to wait for the lock to become available.</param>
    /// <param name="retry">Time between retry attempts.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A lock handle if acquired.</returns>
    /// <exception cref="DistributedLockException">Thrown if the lock could not be acquired within the wait time.</exception>
    Task<IDistributedLockHandle> AcquireAsync(
        string resource,
        TimeSpan expiry,
        TimeSpan wait,
        TimeSpan? retry = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks if a resource is currently locked.
    /// </summary>
    /// <param name="resource">The resource key to check.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>True if the resource is locked.</returns>
    Task<bool> IsLockedAsync(string resource, CancellationToken cancellationToken = default);

    /// <summary>
    /// Executes an action while holding a distributed lock.
    /// The lock is automatically released after the action completes.
    /// </summary>
    /// <typeparam name="T">The return type of the action.</typeparam>
    /// <param name="resource">The resource key to lock.</param>
    /// <param name="action">The action to execute while holding the lock.</param>
    /// <param name="expiry">How long the lock should be held before auto-expiring.</param>
    /// <param name="wait">Maximum time to wait for the lock to become available.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The result of the action.</returns>
    Task<T> ExecuteWithLockAsync<T>(
        string resource,
        Func<CancellationToken, Task<T>> action,
        TimeSpan expiry,
        TimeSpan wait,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Executes an action while holding a distributed lock (void version).
    /// </summary>
    /// <param name="resource">The resource key to lock.</param>
    /// <param name="action">The action to execute while holding the lock.</param>
    /// <param name="expiry">How long the lock should be held before auto-expiring.</param>
    /// <param name="wait">Maximum time to wait for the lock to become available.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    Task ExecuteWithLockAsync(
        string resource,
        Func<CancellationToken, Task> action,
        TimeSpan expiry,
        TimeSpan wait,
        CancellationToken cancellationToken = default);
}

/// <summary>
/// Exception thrown when a distributed lock cannot be acquired.
/// </summary>
public class DistributedLockException : Exception
{
    /// <summary>
    /// The resource that could not be locked.
    /// </summary>
    public string Resource { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="DistributedLockException"/> class.
    /// </summary>
    /// <param name="resource">The resource that could not be locked.</param>
    public DistributedLockException(string resource)
        : base($"Could not acquire lock on resource: {resource}")
    {
        Resource = resource;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="DistributedLockException"/> class.
    /// </summary>
    /// <param name="resource">The resource that could not be locked.</param>
    /// <param name="message">The error message.</param>
    public DistributedLockException(string resource, string message)
        : base(message)
    {
        Resource = resource;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="DistributedLockException"/> class.
    /// </summary>
    /// <param name="resource">The resource that could not be locked.</param>
    /// <param name="message">The error message.</param>
    /// <param name="innerException">The inner exception.</param>
    public DistributedLockException(string resource, string message, Exception innerException)
        : base(message, innerException)
    {
        Resource = resource;
    }
}

/// <summary>
/// Configuration options for distributed locking.
/// </summary>
public class DistributedLockOptions
{
    /// <summary>
    /// Configuration section name in appsettings.json.
    /// </summary>
    public const string SectionName = "DistributedLock";

    /// <summary>
    /// Default lock expiry time in seconds. Default: 30.
    /// </summary>
    public int DefaultExpirySeconds { get; set; } = 30;

    /// <summary>
    /// Default wait time for lock acquisition in seconds. Default: 10.
    /// </summary>
    public int DefaultWaitSeconds { get; set; } = 10;

    /// <summary>
    /// Default retry interval in milliseconds. Default: 100.
    /// </summary>
    public int RetryIntervalMs { get; set; } = 100;

    /// <summary>
    /// Key prefix for Redis lock keys. Default: "lock:".
    /// </summary>
    public string KeyPrefix { get; set; } = "lock:";

    /// <summary>
    /// Whether to enable detailed logging of lock operations.
    /// </summary>
    public bool EnableDetailedLogging { get; set; } = true;
}
