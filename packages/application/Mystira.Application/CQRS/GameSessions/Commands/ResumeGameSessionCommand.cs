using Mystira.Domain.Models;

namespace Mystira.Application.CQRS.GameSessions.Commands;

/// <summary>
/// Command to resume a paused game session
/// </summary>
/// <param name="SessionId">The unique identifier of the game session to resume.</param>
public record ResumeGameSessionCommand(string SessionId) : ICommand<GameSession?>;
