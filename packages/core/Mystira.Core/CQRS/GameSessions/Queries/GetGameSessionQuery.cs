using Mystira.Domain.Models;
using Mystira.Domain.Enums;
using Mystira.Domain.ValueObjects;

namespace Mystira.Core.CQRS.GameSessions.Queries;

/// <summary>
/// Query to retrieve a game session by ID
/// </summary>
public record GetGameSessionQuery(string SessionId) : IQuery<GameSession?>;
