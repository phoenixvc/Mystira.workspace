using Mystira.StoryGenerator.Application.Scenarios;
using Mystira.StoryGenerator.Application.StoryConsistencyAnalysis;
using Mystira.StoryGenerator.Contracts.Entities;
using Mystira.StoryGenerator.Domain.Stories;

namespace Mystira.StoryGenerator.Application.Tests;

public class ScenarioEntityIntroductionValidatorTests
{
    [Fact]
    public void ScenarioEntityIntroductionValidator_ReturnsCorrectly()
    {
        // Arrange: Build a minimal branching scenario
        // S0 (root) -> S1 and S2
        var s0 = new Scene
        {
            Id = "S0",
            Title = "Start",
            Type = SceneType.Choice,
            Description = "The story begins.",
            Branches =
            [
                new Branch { Choice = "Go left", NextSceneId = "S1" },
                new Branch { Choice = "Go right", NextSceneId = "S2" }
            ]
        };

        var s1 = new Scene
        {
            Id = "S1",
            Title = "Left Path",
            Type = SceneType.Narrative,
            Description = "Mentions Mira who was introduced."
        };

        var s2 = new Scene
        {
            Id = "S2",
            Title = "Right Path",
            Type = SceneType.Narrative,
            Description = "Mentions Old Rurik who was not introduced."
        };

        var scenario = new Scenario
        {
            Id = "TestScenario",
            Title = "Test",
            Scenes = new List<Scene> { s0, s1, s2 }
        };

        var graph = ScenarioGraph.FromScenario(scenario);

        // Entities used in the test
        var mira = new SceneEntity { Type = SceneEntityType.Character, Name = "Mira", IsProperNoun = true, Confidence = Confidence.Medium };
        var oldRurik = new SceneEntity { Type = SceneEntityType.Character, Name = "Old Rurik", IsProperNoun = true, Confidence = Confidence.Medium };

        // Introduced/Removed/Used delegates
        IEnumerable<SceneEntity> GetIntroduced(Scene scene)
        {
            // Introduce Mira at the very start
            return scene.Id == "S0" ? new[] { mira } : Array.Empty<SceneEntity>();
        }

        IEnumerable<SceneEntity> GetRemoved(Scene scene)
        {
            // No removals in this simple case
            return Array.Empty<SceneEntity>();
        }

        IEnumerable<SceneEntity> GetUsed(Scene scene)
        {
            return scene.Id switch
            {
                "S1" => new[] { mira },       // valid: Mira was introduced at S0 on all paths
                "S2" => new[] { oldRurik },   // violation: Old Rurik never introduced
                _ => Array.Empty<SceneEntity>()
            };
        }

        // Act
        var violations = ScenarioEntityIntroductionValidator.FindIntroductionViolations(
            graph,
            GetIntroduced,
            GetRemoved,
            GetUsed);

        // Assert: exactly one violation at S2 for Old Rurik
        Assert.NotNull(violations);
        Assert.Single(violations);
        var v = violations[0];
        Assert.Equal("S2", v.SceneId);
        Assert.Equal("Old Rurik", v.Entity.Name);
        Assert.Equal(SceneEntityType.Character, v.Entity.Type);
    }

    [Fact]
    public void ScenarioEntityIntroductionValidator_RemovalHandledCorrectly()
    {
        // Arrange: Linear scenario S0 -> S1 -> S2
        var s0 = new Scene
        {
            Id = "S0",
            Title = "Start",
            Type = SceneType.Narrative,
            Description = "Introduce Mira."
        };

        var s1 = new Scene
        {
            Id = "S1",
            Title = "Middle",
            Type = SceneType.Narrative,
            Description = "Mira departs forever.",
            NextSceneId = "S2"
        };

        var s2 = new Scene
        {
            Id = "S2",
            Title = "End",
            Type = SceneType.Narrative,
            Description = "Mentions Mira after removal.",
        };

        // Wire branches: S0 -> S1, S1 -> S2
        s0.NextSceneId = "S1";

        var scenario = new Scenario
        {
            Id = "RemovalScenario",
            Title = "Removal",
            Scenes = new List<Scene> { s0, s1, s2 }
        };

        var graph = ScenarioGraph.FromScenario(scenario);

        var mira = new SceneEntity { Type = SceneEntityType.Character, Name = "Mira", IsProperNoun = true, Confidence = Confidence.Medium };

        IEnumerable<SceneEntity> GetIntroduced(Scene scene)
            => scene.Id == "S0" ? new[] { mira } : Array.Empty<SceneEntity>();

        IEnumerable<SceneEntity> GetRemoved(Scene scene)
            => scene.Id == "S1" ? new[] { mira } : Array.Empty<SceneEntity>();

        IEnumerable<SceneEntity> GetUsed(Scene scene)
            => scene.Id == "S2" ? new[] { mira } : Array.Empty<SceneEntity>();

        // Act
        var violations = ScenarioEntityIntroductionValidator.FindIntroductionViolations(
            graph,
            GetIntroduced,
            GetRemoved,
            GetUsed);

        // Assert: one violation at S2 because Mira was removed at S1
        Assert.NotNull(violations);
        Assert.Single(violations);
        var v = violations[0];
        Assert.Equal("S2", v.SceneId);
        Assert.Equal("Mira", v.Entity.Name);
        Assert.Equal(SceneEntityType.Character, v.Entity.Type);
    }
}
