using Ardalis.Specification;
using Mystira.Domain.Models;

namespace Mystira.Application.Specifications;

/// <summary>
/// Specification to get a game session by ID.
/// </summary>
public sealed class GameSessionByIdSpec : SingleEntitySpecification<GameSession>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="GameSessionByIdSpec"/> class.
    /// </summary>
    /// <param name="id">The game session identifier.</param>
    public GameSessionByIdSpec(string id)
    {
        Query.Where(s => s.Id == id);
    }
}

/// <summary>
/// Specification to filter sessions by account ID.
/// Migrated from SessionsByAccountSpecification.
/// </summary>
public sealed class SessionsByAccountSpec : BaseEntitySpecification<GameSession>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="SessionsByAccountSpec"/> class.
    /// </summary>
    /// <param name="accountId">The account identifier to filter by.</param>
    public SessionsByAccountSpec(string accountId)
    {
        Query.Where(s => s.AccountId == accountId)
             .OrderByDescending(s => s.StartTime);
    }
}

/// <summary>
/// Specification to filter sessions by profile ID.
/// Migrated from SessionsByProfileSpecification.
/// </summary>
public sealed class SessionsByProfileSpec : BaseEntitySpecification<GameSession>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="SessionsByProfileSpec"/> class.
    /// </summary>
    /// <param name="profileId">The profile identifier to filter by.</param>
    public SessionsByProfileSpec(string profileId)
    {
        Query.Where(s => s.ProfileId == profileId)
             .OrderByDescending(s => s.StartTime);
    }
}

/// <summary>
/// Specification to filter in-progress and paused sessions for an account.
/// Migrated from InProgressSessionsSpecification.
/// </summary>
public sealed class InProgressSessionsSpec : BaseEntitySpecification<GameSession>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="InProgressSessionsSpec"/> class.
    /// </summary>
    /// <param name="accountId">The account identifier to filter by.</param>
    public InProgressSessionsSpec(string accountId)
    {
        Query.Where(s => s.AccountId == accountId &&
                        (s.Status == SessionStatus.InProgress || s.Status == SessionStatus.Paused))
             .OrderByDescending(s => s.StartTime);
    }
}

/// <summary>
/// Specification to filter sessions by scenario ID.
/// Migrated from SessionsByScenarioSpecification.
/// </summary>
public sealed class SessionsByScenarioSpec : BaseEntitySpecification<GameSession>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="SessionsByScenarioSpec"/> class.
    /// </summary>
    /// <param name="scenarioId">The scenario identifier to filter by.</param>
    public SessionsByScenarioSpec(string scenarioId)
    {
        Query.Where(s => s.ScenarioId == scenarioId)
             .OrderByDescending(s => s.StartTime);
    }
}

/// <summary>
/// Specification to filter active sessions (in progress or paused).
/// Migrated from ActiveSessionsSpecification.
/// </summary>
public sealed class ActiveSessionsSpec : BaseEntitySpecification<GameSession>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ActiveSessionsSpec"/> class.
    /// </summary>
    public ActiveSessionsSpec()
    {
        Query.Where(s => s.Status == SessionStatus.InProgress || s.Status == SessionStatus.Paused)
             .OrderByDescending(s => s.StartTime);
    }
}

/// <summary>
/// Specification to filter completed sessions.
/// Migrated from CompletedSessionsSpecification.
/// </summary>
public sealed class CompletedSessionsSpec : BaseEntitySpecification<GameSession>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="CompletedSessionsSpec"/> class.
    /// </summary>
    public CompletedSessionsSpec()
    {
        Query.Where(s => s.Status == SessionStatus.Completed)
             .OrderByDescending(s => s.EndTime ?? s.StartTime);
    }
}

/// <summary>
/// Specification to filter sessions by status.
/// Migrated from SessionsByStatusSpecification.
/// </summary>
public sealed class SessionsByStatusSpec : BaseEntitySpecification<GameSession>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="SessionsByStatusSpec"/> class.
    /// </summary>
    /// <param name="status">The session status to filter by.</param>
    public SessionsByStatusSpec(SessionStatus status)
    {
        Query.Where(s => s.Status == status)
             .OrderByDescending(s => s.StartTime);
    }
}

/// <summary>
/// Specification to filter sessions by account and scenario.
/// Migrated from SessionsByAccountAndScenarioSpecification.
/// </summary>
public sealed class SessionsByAccountAndScenarioSpec : BaseEntitySpecification<GameSession>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="SessionsByAccountAndScenarioSpec"/> class.
    /// </summary>
    /// <param name="accountId">The account identifier to filter by.</param>
    /// <param name="scenarioId">The scenario identifier to filter by.</param>
    public SessionsByAccountAndScenarioSpec(string accountId, string scenarioId)
    {
        Query.Where(s => s.AccountId == accountId && s.ScenarioId == scenarioId)
             .OrderByDescending(s => s.StartTime);
    }
}

/// <summary>
/// Specification for paginated game sessions with filters.
/// </summary>
public sealed class GameSessionsPaginatedSpec : BaseEntitySpecification<GameSession>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="GameSessionsPaginatedSpec"/> class.
    /// </summary>
    /// <param name="skip">The number of records to skip.</param>
    /// <param name="take">The number of records to take.</param>
    /// <param name="accountId">Optional account identifier to filter by.</param>
    /// <param name="profileId">Optional profile identifier to filter by.</param>
    /// <param name="status">Optional session status to filter by.</param>
    public GameSessionsPaginatedSpec(
        int skip,
        int take,
        string? accountId = null,
        string? profileId = null,
        SessionStatus? status = null)
    {
        var query = Query.AsTracking();

        if (!string.IsNullOrWhiteSpace(accountId))
        {
            query = query.Where(s => s.AccountId == accountId);
        }

        if (!string.IsNullOrWhiteSpace(profileId))
        {
            query = query.Where(s => s.ProfileId == profileId);
        }

        if (status.HasValue)
        {
            query = query.Where(s => s.Status == status.Value);
        }

        query.OrderByDescending(s => s.StartTime)
             .Skip(skip)
             .Take(take);
    }
}
