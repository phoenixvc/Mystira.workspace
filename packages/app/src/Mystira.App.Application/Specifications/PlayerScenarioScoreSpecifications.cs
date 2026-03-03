using Ardalis.Specification;
using Mystira.App.Domain.Models;

namespace Mystira.App.Application.Specifications;

public sealed class PlayerScenarioScoreByProfileAndScenarioSpec : SingleResultSpecification<PlayerScenarioScore>
{
    public PlayerScenarioScoreByProfileAndScenarioSpec(string profileId, string scenarioId)
    {
        Query.Where(s =>
            s.ProfileId == profileId &&
            s.ScenarioId == scenarioId);
    }
}

public sealed class PlayerScenarioScoresByProfileSpec : Specification<PlayerScenarioScore>
{
    public PlayerScenarioScoresByProfileSpec(string profileId)
    {
        Query
            .Where(s => s.ProfileId == profileId)
            .OrderByDescending(s => s.CreatedAt);
    }
}

public sealed class PlayerScenarioScoreExistsSpec : SingleResultSpecification<PlayerScenarioScore>
{
    public PlayerScenarioScoreExistsSpec(string profileId, string scenarioId)
    {
        Query.Where(s =>
            s.ProfileId == profileId &&
            s.ScenarioId == scenarioId);
    }
}
