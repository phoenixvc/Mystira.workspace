using Microsoft.Extensions.Logging;
using Mystira.App.Application.UseCases.GameSessions;
using Mystira.App.Domain.Models;

namespace Mystira.App.Application.CQRS.GameSessions.Commands;

/// <summary>
/// Wolverine handler for StartGameSessionCommand.
/// Delegates all business logic to CreateGameSessionUseCase.
/// </summary>
public static class StartGameSessionCommandHandler
{
    public static async Task<GameSession?> Handle(
        StartGameSessionCommand command,
        ICreateGameSessionUseCase useCase,
        ILogger logger,
        CancellationToken ct)
    {
        var result = await useCase.ExecuteAsync(command.Request, ct);

        if (!result.IsSuccess)
        {
            logger.LogWarning("StartGameSession failed: {Error}", result.ErrorMessage);
            return null;
        }

        return result.Data;
    }
}
