using Mystira.Domain.Models;
using Mystira.Domain.Enums;
using Mystira.Domain.ValueObjects;

namespace Mystira.Core.CQRS.GameSessions.Commands;

/// <summary>
/// Command to end an active game session
/// </summary>
public record EndGameSessionCommand(string SessionId) : ICommand<GameSession?>;
