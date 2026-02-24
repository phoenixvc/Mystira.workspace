using Mystira.StoryGenerator.Domain.Agents;

namespace Mystira.StoryGenerator.Application.Infrastructure.Agents;

/// <summary>
/// Repository interface for story session persistence.
/// </summary>
public interface IStorySessionRepository
{
    /// <summary>
    /// Creates a new story session.
    /// </summary>
    /// <param name="session">The session to create.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The created session.</returns>
    Task<StorySession> CreateAsync(StorySession session, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a story session by its ID.
    /// </summary>
    /// <param name="sessionId">The session ID.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The session or null if not found.</returns>
    Task<StorySession?> GetAsync(string sessionId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates an existing story session.
    /// </summary>
    /// <param name="session">The session to update.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The updated session.</returns>
    Task<StorySession> UpdateAsync(StorySession session, CancellationToken cancellationToken = default);

    /// <summary>
    /// Upserts a story session (creates if not exists, updates if exists).
    /// </summary>
    /// <param name="session">The session to upsert.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The upserted session.</returns>
    Task<StorySession> UpsertAsync(StorySession session, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a story session by its Foundry thread ID.
    /// </summary>
    /// <param name="threadId">The Foundry thread ID.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The session or null if not found.</returns>
    Task<StorySession?> GetByThreadIdAsync(string threadId, CancellationToken cancellationToken = default);
}
