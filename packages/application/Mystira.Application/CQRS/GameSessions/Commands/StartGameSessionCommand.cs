using Mystira.Contracts.App.Requests.GameSessions;
using Mystira.Domain.Models;

namespace Mystira.Application.CQRS.GameSessions.Commands;

/// <summary>
/// Command to start a new game session
/// </summary>
/// <param name="Request">The request containing the game session initialization data.</param>
public record StartGameSessionCommand(StartGameSessionRequest Request) : ICommand<GameSession>;
