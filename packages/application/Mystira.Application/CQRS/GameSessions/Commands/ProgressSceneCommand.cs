using Mystira.Contracts.App.Requests.GameSessions;
using Mystira.Domain.Models;

namespace Mystira.Application.CQRS.GameSessions.Commands;

/// <summary>
/// Command to progress a game session to a new scene
/// </summary>
/// <param name="Request">The request containing the scene progression data.</param>
public record ProgressSceneCommand(ProgressSceneRequest Request) : ICommand<GameSession?>;
