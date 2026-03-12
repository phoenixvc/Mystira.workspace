using Mystira.Contracts.App.Requests.GameSessions;
using Mystira.Domain.Models;
using Mystira.Domain.Enums;
using Mystira.Domain.ValueObjects;

namespace Mystira.Core.CQRS.GameSessions.Commands;

/// <summary>
/// Command to start a new game session
/// </summary>
public record StartGameSessionCommand(StartGameSessionRequest Request) : ICommand<GameSession>;
