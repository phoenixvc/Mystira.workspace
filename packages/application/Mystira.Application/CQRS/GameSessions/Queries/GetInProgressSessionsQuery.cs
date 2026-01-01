using Mystira.Contracts.App.Responses.GameSessions;

namespace Mystira.Application.CQRS.GameSessions.Queries;

/// <summary>
/// Query to retrieve in-progress and paused sessions for an account
/// </summary>
/// <param name="AccountId">The unique identifier of the account.</param>
public record GetInProgressSessionsQuery(string AccountId) : IQuery<List<GameSessionResponse>>;
