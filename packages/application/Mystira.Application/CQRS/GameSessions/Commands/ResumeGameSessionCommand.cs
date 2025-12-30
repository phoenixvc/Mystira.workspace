using Mystira.Domain.Models;

namespace Mystira.Application.CQRS.GameSessions.Commands;

/// <summary>
/// Command to resume a paused game session
/// </summary>
public record ResumeGameSessionCommand(string SessionId) : ICommand<GameSession?>;
