using Mystira.Core.UseCases.GameSessions;
using Mystira.Domain.Models;
using Mystira.Domain.Enums;
using Mystira.Domain.ValueObjects;

namespace Mystira.Core.CQRS.GameSessions.Commands;

/// <summary>
/// Wolverine handler for EndGameSessionCommand.
/// Delegates to EndGameSessionUseCase which owns the full business logic including
/// input validation, already-completed check, ElapsedTime calculation, and pause state cleanup.
/// </summary>
public static class EndGameSessionCommandHandler
{
    /// <summary>
    /// Handles the EndGameSessionCommand by delegating to the UseCase.
    /// Wolverine injects the UseCase as a method parameter.
    /// </summary>
    public static async Task<GameSession?> Handle(
        EndGameSessionCommand command,
        EndGameSessionUseCase endGameSessionUseCase,
        CancellationToken ct)
    {
        try
        {
            return await endGameSessionUseCase.ExecuteAsync(command.SessionId);
        }
        catch (ArgumentException)
        {
            // UseCase throws ArgumentException for not-found or empty session IDs.
            // Handler preserves nullable return contract for backward compatibility.
            return null;
        }
    }
}
