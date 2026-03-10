using Microsoft.EntityFrameworkCore;
using Mystira.App.Application.Ports.Data;
using Mystira.Domain.Models;
using Mystira.Domain.Enums;
using Mystira.Domain.ValueObjects;

namespace Mystira.App.Infrastructure.Data.Repositories;

public class PlayerScenarioScoreRepository : Repository<PlayerScenarioScore>, IPlayerScenarioScoreRepository
{
    public PlayerScenarioScoreRepository(MystiraAppDbContext context)
        : base(context)
    {
    }

    public async Task<PlayerScenarioScore?> GetByProfileAndScenarioAsync(string profileId, string scenarioId, CancellationToken ct = default)
    {
        return await ((MystiraAppDbContext)_context).PlayerScenarioScores
            .FirstOrDefaultAsync(x => x.ProfileId == profileId && x.ScenarioId == scenarioId, ct);
    }

    public async Task<IEnumerable<PlayerScenarioScore>> GetByProfileIdAsync(string profileId, CancellationToken ct = default)
    {
        return await ((MystiraAppDbContext)_context).PlayerScenarioScores
            .Where(x => x.ProfileId == profileId)
            .OrderByDescending(x => x.CreatedAt)
            .ToListAsync(ct);
    }

    public async Task<bool> IsScenarioScoredAsync(string profileId, string scenarioId, CancellationToken ct = default)
    {
        return await ((MystiraAppDbContext)_context).PlayerScenarioScores
            .AnyAsync(x => x.ProfileId == profileId && x.ScenarioId == scenarioId, ct);
    }
}
