using Microsoft.EntityFrameworkCore;
using Mystira.Application.Ports.Data;
using Mystira.Domain.Models;

namespace Mystira.Infrastructure.Data.Repositories;

/// <summary>
/// Repository implementation for PlayerScenarioScore entity with domain-specific queries.
/// </summary>
public class PlayerScenarioScoreRepository : Repository<PlayerScenarioScore>, IPlayerScenarioScoreRepository
{
    /// <summary>
    /// Initializes a new instance of the <see cref="PlayerScenarioScoreRepository"/> class.
    /// </summary>
    /// <param name="context">The database context.</param>
    public PlayerScenarioScoreRepository(MystiraAppDbContext context)
        : base(context)
    {
    }

    /// <summary>
    /// Retrieves a player's score for a specific scenario.
    /// </summary>
    /// <param name="profileId">The player profile ID.</param>
    /// <param name="scenarioId">The scenario ID.</param>
    /// <returns>The player scenario score, or null if not found.</returns>
    public async Task<PlayerScenarioScore?> GetByProfileAndScenarioAsync(string profileId, string scenarioId)
    {
        return await ((MystiraAppDbContext)_context).PlayerScenarioScores
            .FirstOrDefaultAsync(x => x.ProfileId == profileId && x.ScenarioId == scenarioId);
    }

    /// <summary>
    /// Retrieves all scenario scores for a specific player profile, ordered by creation date descending.
    /// </summary>
    /// <param name="profileId">The player profile ID.</param>
    /// <returns>A collection of player scenario scores.</returns>
    public async Task<IEnumerable<PlayerScenarioScore>> GetByProfileIdAsync(string profileId)
    {
        return await ((MystiraAppDbContext)_context).PlayerScenarioScores
            .Where(x => x.ProfileId == profileId)
            .OrderByDescending(x => x.CreatedAt)
            .ToListAsync();
    }

    /// <summary>
    /// Checks if a player has scored a specific scenario.
    /// </summary>
    /// <param name="profileId">The player profile ID.</param>
    /// <param name="scenarioId">The scenario ID.</param>
    /// <returns>True if the player has scored the scenario; otherwise, false.</returns>
    public async Task<bool> IsScenarioScoredAsync(string profileId, string scenarioId)
    {
        return await ((MystiraAppDbContext)_context).PlayerScenarioScores
            .AnyAsync(x => x.ProfileId == profileId && x.ScenarioId == scenarioId);
    }
}
