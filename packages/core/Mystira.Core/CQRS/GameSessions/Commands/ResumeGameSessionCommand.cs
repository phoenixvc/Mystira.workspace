using Mystira.Domain.Models;
using Mystira.Domain.Enums;
using Mystira.Domain.ValueObjects;

namespace Mystira.Core.CQRS.GameSessions.Commands;

/// <summary>
/// Command to resume a paused game session
/// </summary>
public record ResumeGameSessionCommand(string SessionId) : ICommand<GameSession?>;
