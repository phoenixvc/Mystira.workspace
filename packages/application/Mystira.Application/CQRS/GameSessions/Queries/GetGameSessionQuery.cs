using Mystira.Domain.Models;

namespace Mystira.Application.CQRS.GameSessions.Queries;

/// <summary>
/// Query to retrieve a game session by ID
/// </summary>
/// <param name="SessionId">The unique identifier of the game session.</param>
public record GetGameSessionQuery(string SessionId) : IQuery<GameSession?>;
