using Mystira.Core.UseCases.GameSessions;
using Mystira.Domain.Models;
using Mystira.Domain.Enums;
using Mystira.Domain.ValueObjects;

namespace Mystira.Core.CQRS.GameSessions.Commands;

/// <summary>
/// Wolverine handler for MakeChoiceCommand.
/// Delegates to MakeChoiceUseCase which owns the full business logic including
/// scenario/scene validation, ActiveCharacter resolution, echo logs, and auto-completion.
/// </summary>
public static class MakeChoiceCommandHandler
{
    /// <summary>
    /// Handles the MakeChoiceCommand by delegating to the UseCase.
    /// Wolverine injects the UseCase as a method parameter.
    /// </summary>
    public static async Task<GameSession?> Handle(
        MakeChoiceCommand command,
        MakeChoiceUseCase makeChoiceUseCase,
        CancellationToken ct)
    {
        return await makeChoiceUseCase.ExecuteAsync(command.Request);
    }
}
