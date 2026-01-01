using Mystira.Domain.Models;

namespace Mystira.Application.CQRS.GameSessions.Commands;

/// <summary>
/// Command to end an active game session
/// </summary>
/// <param name="SessionId">The unique identifier of the game session to end.</param>
public record EndGameSessionCommand(string SessionId) : ICommand<GameSession?>;
