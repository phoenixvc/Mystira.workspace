using Mystira.Contracts.App.Requests.GameSessions;
using Mystira.Domain.Models;
using Mystira.Domain.Enums;
using Mystira.Domain.ValueObjects;

namespace Mystira.App.Application.CQRS.GameSessions.Commands;

/// <summary>
/// Command to record a choice made during a game session
/// </summary>
public record MakeChoiceCommand(MakeChoiceRequest Request) : ICommand<GameSession?>;
