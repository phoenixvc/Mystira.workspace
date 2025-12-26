using Microsoft.Extensions.Logging;

namespace Mystira.Shared.Extensions;

/// <summary>
/// Extensions for safe async operations.
/// Prevents fire-and-forget patterns and ensures proper error handling.
/// </summary>
public static class AsyncExtensions
{
    /// <summary>
    /// Safely executes a task with proper error handling.
    /// Use this instead of fire-and-forget patterns.
    /// </summary>
    /// <param name="task">The task to execute.</param>
    /// <param name="logger">Logger for error reporting.</param>
    /// <param name="operationName">Name of the operation for logging.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    public static async Task SafeExecuteAsync(
        this Task task,
        ILogger logger,
        string operationName,
        CancellationToken cancellationToken = default)
    {
        try
        {
            await task.WaitAsync(cancellationToken);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            logger.LogInformation("Operation '{OperationName}' was cancelled", operationName);
        }
        catch (TaskCanceledException ex)
        {
            logger.LogWarning(ex, "Operation '{OperationName}' was cancelled unexpectedly", operationName);
        }
        catch (TimeoutException ex)
        {
            logger.LogWarning(ex, "Operation '{OperationName}' timed out", operationName);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Operation '{OperationName}' failed", operationName);
        }
    }

    /// <summary>
    /// Safely executes a task with proper error handling and returns a result.
    /// </summary>
    /// <typeparam name="T">The result type.</typeparam>
    /// <param name="task">The task to execute.</param>
    /// <param name="logger">Logger for error reporting.</param>
    /// <param name="operationName">Name of the operation for logging.</param>
    /// <param name="defaultValue">Default value on failure.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The result or default value on failure.</returns>
    public static async Task<T?> SafeExecuteAsync<T>(
        this Task<T> task,
        ILogger logger,
        string operationName,
        T? defaultValue = default,
        CancellationToken cancellationToken = default)
    {
        try
        {
            return await task.WaitAsync(cancellationToken);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            logger.LogInformation("Operation '{OperationName}' was cancelled", operationName);
            return defaultValue;
        }
        catch (TaskCanceledException ex)
        {
            logger.LogWarning(ex, "Operation '{OperationName}' was cancelled unexpectedly", operationName);
            return defaultValue;
        }
        catch (TimeoutException ex)
        {
            logger.LogWarning(ex, "Operation '{OperationName}' timed out", operationName);
            return defaultValue;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Operation '{OperationName}' failed", operationName);
            return defaultValue;
        }
    }

    /// <summary>
    /// Executes a task with a timeout.
    /// </summary>
    /// <param name="task">The task to execute.</param>
    /// <param name="timeout">Maximum time to wait.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <exception cref="TimeoutException">Thrown if the task exceeds the timeout.</exception>
    public static async Task WithTimeoutAsync(
        this Task task,
        TimeSpan timeout,
        CancellationToken cancellationToken = default)
    {
        using var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        cts.CancelAfter(timeout);

        try
        {
            await task.WaitAsync(cts.Token);
        }
        catch (OperationCanceledException) when (!cancellationToken.IsCancellationRequested)
        {
            throw new TimeoutException($"Operation timed out after {timeout.TotalSeconds} seconds");
        }
    }

    /// <summary>
    /// Executes a task with a timeout and returns a result.
    /// </summary>
    /// <typeparam name="T">The result type.</typeparam>
    /// <param name="task">The task to execute.</param>
    /// <param name="timeout">Maximum time to wait.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The result.</returns>
    /// <exception cref="TimeoutException">Thrown if the task exceeds the timeout.</exception>
    public static async Task<T> WithTimeoutAsync<T>(
        this Task<T> task,
        TimeSpan timeout,
        CancellationToken cancellationToken = default)
    {
        using var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        cts.CancelAfter(timeout);

        try
        {
            return await task.WaitAsync(cts.Token);
        }
        catch (OperationCanceledException) when (!cancellationToken.IsCancellationRequested)
        {
            throw new TimeoutException($"Operation timed out after {timeout.TotalSeconds} seconds");
        }
    }

    /// <summary>
    /// Ensures a CancellationToken is provided, using a default timeout if none.
    /// Note: The returned CancellationTokenSource should be disposed by the caller.
    /// </summary>
    /// <param name="cancellationToken">The provided token.</param>
    /// <param name="defaultTimeout">Default timeout if no token provided.</param>
    /// <param name="cts">Output parameter for the created CancellationTokenSource (null if input token was used).</param>
    /// <returns>A valid cancellation token.</returns>
    public static CancellationToken EnsureToken(
        this CancellationToken cancellationToken,
        TimeSpan? defaultTimeout,
        out CancellationTokenSource? cts)
    {
        if (cancellationToken != default)
        {
            cts = null;
            return cancellationToken;
        }

        // Create a token with default 30 second timeout
        var timeout = defaultTimeout ?? TimeSpan.FromSeconds(30);
        cts = new CancellationTokenSource(timeout);
        return cts.Token;
    }
}
