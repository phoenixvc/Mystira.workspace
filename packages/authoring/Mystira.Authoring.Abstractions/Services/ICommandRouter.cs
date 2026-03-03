using Mystira.Authoring.Abstractions.Commands;

namespace Mystira.Authoring.Abstractions.Services;

/// <summary>
/// Routes user input to the appropriate command handler.
/// </summary>
public interface ICommandRouter
{
    /// <summary>
    /// Determines which command to execute based on user input.
    /// </summary>
    /// <param name="userInput">The user's input text.</param>
    /// <param name="context">The current authoring context.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>The command to execute, or null if no matching command found.</returns>
    Task<ICommand?> RouteAsync(
        string userInput,
        AuthoringContext context,
        CancellationToken cancellationToken = default);
}
