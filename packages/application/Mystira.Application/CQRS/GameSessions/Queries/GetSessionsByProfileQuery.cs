using Mystira.Contracts.App.Responses.GameSessions;

namespace Mystira.Application.CQRS.GameSessions.Queries;

/// <summary>
/// Query to retrieve all game sessions for a specific profile
/// </summary>
/// <param name="ProfileId">The unique identifier of the user profile.</param>
public record GetSessionsByProfileQuery(string ProfileId) : IQuery<List<GameSessionResponse>>;
