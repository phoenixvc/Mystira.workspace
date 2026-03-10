using Mystira.Domain.Models;
using Mystira.Domain.Enums;
using Mystira.Domain.ValueObjects;

namespace Mystira.App.Application.CQRS.GameSessions.Commands;

/// <summary>
/// Command to pause an active game session
/// </summary>
public record PauseGameSessionCommand(string SessionId) : ICommand<GameSession?>;
