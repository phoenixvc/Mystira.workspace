using Mystira.Contracts.App.Responses.GameSessions;

namespace Mystira.Core.CQRS.GameSessions.Queries;

/// <summary>
/// Query to retrieve session statistics and analytics
/// </summary>
/// <param name="SessionId">The unique identifier of the game session.</param>
public record GetSessionStatsQuery(string SessionId) : IQuery<SessionStatsResponse?>;
