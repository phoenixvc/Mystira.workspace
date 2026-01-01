using Mystira.Domain.Models;

namespace Mystira.Application.CQRS.GameSessions.Commands;

/// <summary>
/// Command to pause an active game session
/// </summary>
/// <param name="SessionId">The unique identifier of the game session to pause.</param>
public record PauseGameSessionCommand(string SessionId) : ICommand<GameSession?>;
