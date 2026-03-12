using Ardalis.Specification;
using Mystira.Domain.Models;

namespace Mystira.Core.Specifications;

/// <summary>Find a player scenario score by profile and scenario ID.</summary>
public sealed class PlayerScenarioScoreByProfileAndScenarioSpec : SingleResultSpecification<PlayerScenarioScore>
{
    /// <summary>Initializes a new instance.</summary>
    public PlayerScenarioScoreByProfileAndScenarioSpec(string profileId, string scenarioId)
    {
        Query.Where(s =>
            s.ProfileId == profileId &&
            s.ScenarioId == scenarioId);
    }
}

/// <summary>Find player scenario scores by profile ID.</summary>
public sealed class PlayerScenarioScoresByProfileSpec : Specification<PlayerScenarioScore>
{
    /// <summary>Initializes a new instance.</summary>
    public PlayerScenarioScoresByProfileSpec(string profileId)
    {
        Query
            .Where(s => s.ProfileId == profileId)
            .OrderByDescending(s => s.CreatedAt);
    }
}

/// <summary>Check if a player scenario score exists for a profile and scenario.</summary>
public sealed class PlayerScenarioScoreExistsSpec : SingleResultSpecification<PlayerScenarioScore>
{
    /// <summary>Initializes a new instance.</summary>
    public PlayerScenarioScoreExistsSpec(string profileId, string scenarioId)
    {
        Query.Where(s =>
            s.ProfileId == profileId &&
            s.ScenarioId == scenarioId);
    }
}
