using Mystira.Domain.Models;
using Mystira.Domain.Enums;
using Mystira.Domain.ValueObjects;
using Mystira.Contracts.App.Requests.GameSessions;

namespace Mystira.App.Application.UseCases.GameSessions;

/// <summary>
/// Interface for CreateGameSessionUseCase to support dependency inversion
/// in Wolverine handlers and enable testability.
/// </summary>
public interface ICreateGameSessionUseCase
{
    Task<UseCaseResult<GameSession>> ExecuteAsync(StartGameSessionRequest request, CancellationToken ct = default);
}
