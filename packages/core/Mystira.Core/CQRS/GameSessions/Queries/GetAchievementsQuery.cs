using Mystira.Domain.Models;
using Mystira.Domain.Enums;
using Mystira.Domain.ValueObjects;

namespace Mystira.Core.CQRS.GameSessions.Queries;

/// <summary>
/// Query to retrieve achievements for a game session
/// </summary>
public record GetAchievementsQuery(string SessionId) : IQuery<List<SessionAchievement>>;
