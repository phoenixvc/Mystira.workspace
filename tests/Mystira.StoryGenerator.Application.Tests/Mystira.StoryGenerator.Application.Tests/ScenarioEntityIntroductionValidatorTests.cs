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
        var mira = new SceneEntity { Type = SceneEntityType.Character, Name = "Mira" };
        var oldRurik = new SceneEntity { Type = SceneEntityType.Character, Name = "Old Rurik" };

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
}
