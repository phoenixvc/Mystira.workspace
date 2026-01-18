namespace Mystira.Contracts.StoryGenerator.Services;

/// <summary>
/// Generic command handler interface for CQRS pattern.
/// </summary>
/// <typeparam name="TCommand">The type of command to handle.</typeparam>
/// <typeparam name="TResponse">The type of response returned.</typeparam>
public interface ICommandHandler<in TCommand, TResponse>
{
    /// <summary>
    /// Handles the specified command.
    /// </summary>
    /// <param name="command">The command to handle.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The response from handling the command.</returns>
    Task<TResponse> HandleAsync(TCommand command, CancellationToken cancellationToken = default);
}
