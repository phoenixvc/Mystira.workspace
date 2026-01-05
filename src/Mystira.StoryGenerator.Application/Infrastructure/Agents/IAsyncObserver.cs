namespace Mystira.StoryGenerator.Application.Infrastructure.Agents;

/// <summary>
/// Async observer interface for receiving stream events.
/// </summary>
public interface IAsyncObserver<in T>
{
    /// <summary>
    /// Called when an event is received.
    /// </summary>
    /// <param name="evt">The event received.</param>
    Task OnNextAsync(T evt);

    /// <summary>
    /// Called when an error occurs.
    /// </summary>
    /// <param name="error">The error that occurred.</param>
    Task OnErrorAsync(Exception error);

    /// <summary>
    /// Called when the stream is completed.
    /// </summary>
    Task OnCompletedAsync();
}