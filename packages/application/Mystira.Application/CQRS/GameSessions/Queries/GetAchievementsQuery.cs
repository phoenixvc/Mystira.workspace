using Mystira.Domain.Models;

namespace Mystira.Application.CQRS.GameSessions.Queries;

/// <summary>
/// Query to retrieve achievements for a game session
/// </summary>
/// <param name="SessionId">The unique identifier of the game session.</param>
public record GetAchievementsQuery(string SessionId) : IQuery<List<SessionAchievement>>;
