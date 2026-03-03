using Mystira.Contracts.App.Responses.GameSessions;

namespace Mystira.Application.CQRS.GameSessions.Queries;

/// <summary>
/// Query to retrieve all game sessions for a specific account
/// </summary>
/// <param name="AccountId">The unique identifier of the account.</param>
public record GetSessionsByAccountQuery(string AccountId) : IQuery<List<GameSessionResponse>>;
