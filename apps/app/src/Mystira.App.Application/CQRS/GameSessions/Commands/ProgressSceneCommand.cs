using Mystira.Contracts.App.Requests.GameSessions;
using Mystira.Domain.Models;
using Mystira.Domain.Enums;
using Mystira.Domain.ValueObjects;

namespace Mystira.App.Application.CQRS.GameSessions.Commands;

/// <summary>
/// Command to progress a game session to a new scene
/// </summary>
public record ProgressSceneCommand(ProgressSceneRequest Request) : ICommand<GameSession?>;
