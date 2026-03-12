using Mystira.Shared.Locking;

namespace Mystira.Admin.Api.Tests.Infrastructure;

/// <summary>
/// In-memory implementation of IDistributedLockService for integration tests.
/// All locks are immediately granted (no contention in tests).
/// </summary>
public sealed class InMemoryDistributedLockService : IDistributedLockService
{
    public Task<IDistributedLockHandle?> TryAcquireAsync(
        string resource, TimeSpan expiry, CancellationToken cancellationToken = default)
    {
        IDistributedLockHandle handle = new InMemoryLockHandle(resource);
        return Task.FromResult<IDistributedLockHandle?>(handle);
    }

    public Task<IDistributedLockHandle> AcquireAsync(
        string resource, TimeSpan expiry, TimeSpan wait, TimeSpan? retry = null,
        CancellationToken cancellationToken = default)
    {
        IDistributedLockHandle handle = new InMemoryLockHandle(resource);
        return Task.FromResult(handle);
    }

    public Task<bool> IsLockedAsync(string resource, CancellationToken cancellationToken = default)
        => Task.FromResult(false);

    public async Task<T> ExecuteWithLockAsync<T>(
        string resource, Func<CancellationToken, Task<T>> action,
        TimeSpan expiry, TimeSpan wait, CancellationToken cancellationToken = default)
    {
        return await action(cancellationToken);
    }

    public async Task ExecuteWithLockAsync(
        string resource, Func<CancellationToken, Task> action,
        TimeSpan expiry, TimeSpan wait, CancellationToken cancellationToken = default)
    {
        await action(cancellationToken);
    }

    private sealed class InMemoryLockHandle : IDistributedLockHandle
    {
        public string LockId { get; } = Guid.NewGuid().ToString();
        public string Resource { get; }
        public bool IsAcquired => true;
        public DateTimeOffset? AcquiredAt { get; } = DateTimeOffset.UtcNow;
        public DateTimeOffset? ExpiresAt { get; } = DateTimeOffset.UtcNow.AddMinutes(5);

        public InMemoryLockHandle(string resource) => Resource = resource;

        public Task<bool> ExtendAsync(TimeSpan extension, CancellationToken cancellationToken = default)
            => Task.FromResult(true);

        public Task ReleaseAsync(CancellationToken cancellationToken = default)
            => Task.CompletedTask;

        public ValueTask DisposeAsync() => ValueTask.CompletedTask;
    }
}
