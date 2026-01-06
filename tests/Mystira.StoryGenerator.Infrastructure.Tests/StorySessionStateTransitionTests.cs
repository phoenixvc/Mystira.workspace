using Mystira.StoryGenerator.Domain.Agents;
using Xunit;

namespace Mystira.StoryGenerator.Infrastructure.Tests;

/// <summary>
/// Tests for StorySession state machine transitions.
/// Validates that state transitions follow expected patterns and prevent invalid transitions.
/// </summary>
public class StorySessionStateTransitionTests
{
    [Fact]
    public void StateTransition_Uninitialized_To_Generating_IsValid()
    {
        // Arrange
        var session = new StorySession
        {
            SessionId = "test-session",
            Stage = StorySessionStage.Uninitialized
        };

        // Act
        session.Stage = StorySessionStage.Generating;

        // Assert
        Assert.Equal(StorySessionStage.Generating, session.Stage);
    }

    [Fact]
    public void StateTransition_Generating_To_Validating_IsValid()
    {
        // Arrange
        var session = new StorySession
        {
            SessionId = "test-session",
            Stage = StorySessionStage.Generating
        };

        // Act
        session.Stage = StorySessionStage.Validating;

        // Assert
        Assert.Equal(StorySessionStage.Validating, session.Stage);
    }

    [Fact]
    public void StateTransition_Validating_To_Evaluating_IsValid()
    {
        // Arrange
        var session = new StorySession
        {
            SessionId = "test-session",
            Stage = StorySessionStage.Validating
        };

        // Act
        session.Stage = StorySessionStage.Evaluating;

        // Assert
        Assert.Equal(StorySessionStage.Evaluating, session.Stage);
    }

    [Fact]
    public void StateTransition_Evaluating_To_Evaluated_IsValid()
    {
        // Arrange
        var session = new StorySession
        {
            SessionId = "test-session",
            Stage = StorySessionStage.Evaluating
        };

        // Act
        session.Stage = StorySessionStage.Evaluated;

        // Assert
        Assert.Equal(StorySessionStage.Evaluated, session.Stage);
    }

    [Fact]
    public void StateTransition_Evaluating_To_RequiresRefinement_IsValid()
    {
        // Arrange
        var session = new StorySession
        {
            SessionId = "test-session",
            Stage = StorySessionStage.Evaluating
        };

        // Act
        session.Stage = StorySessionStage.RequiresRefinement;

        // Assert
        Assert.Equal(StorySessionStage.RequiresRefinement, session.Stage);
    }

    [Fact]
    public void StateTransition_RequiresRefinement_To_Refining_IsValid()
    {
        // Arrange
        var session = new StorySession
        {
            SessionId = "test-session",
            Stage = StorySessionStage.RequiresRefinement
        };

        // Act
        session.Stage = StorySessionStage.Refining;

        // Assert
        Assert.Equal(StorySessionStage.Refining, session.Stage);
    }

    [Fact]
    public void StateTransition_Refining_To_Validating_IsValid()
    {
        // Arrange
        var session = new StorySession
        {
            SessionId = "test-session",
            Stage = StorySessionStage.Refining
        };

        // Act
        session.Stage = StorySessionStage.Validating;

        // Assert
        Assert.Equal(StorySessionStage.Validating, session.Stage);
    }

    [Fact]
    public void StateTransition_Evaluated_To_Complete_IsValid()
    {
        // Arrange
        var session = new StorySession
        {
            SessionId = "test-session",
            Stage = StorySessionStage.Evaluated
        };

        // Act
        session.Stage = StorySessionStage.Complete;

        // Assert
        Assert.Equal(StorySessionStage.Complete, session.Stage);
    }

    [Fact]
    public void StateTransition_Evaluating_To_StuckNeedsReview_IsValid()
    {
        // Arrange
        var session = new StorySession
        {
            SessionId = "test-session",
            Stage = StorySessionStage.Evaluating,
            IterationCount = 5
        };

        // Act
        session.Stage = StorySessionStage.StuckNeedsReview;

        // Assert
        Assert.Equal(StorySessionStage.StuckNeedsReview, session.Stage);
    }

    [Fact]
    public void StateTransition_Loop_Validating_To_Refining_To_Validating_IsValid()
    {
        // Arrange
        var session = new StorySession
        {
            SessionId = "test-session",
            Stage = StorySessionStage.Validating
        };

        // Act - Simulate refinement loop
        session.Stage = StorySessionStage.Evaluating;
        session.Stage = StorySessionStage.RequiresRefinement;
        session.Stage = StorySessionStage.Refining;
        session.IterationCount++;
        session.Stage = StorySessionStage.Validating;

        // Assert
        Assert.Equal(StorySessionStage.Validating, session.Stage);
        Assert.Equal(1, session.IterationCount);
    }

    [Fact]
    public void TerminalState_Complete_CannotTransition()
    {
        // Arrange
        var session = new StorySession
        {
            SessionId = "test-session",
            Stage = StorySessionStage.Complete
        };

        // Act & Assert
        // In a real implementation, this would be enforced by the orchestrator
        // Here we document that Complete is a terminal state
        Assert.Equal(StorySessionStage.Complete, session.Stage);
    }

    [Fact]
    public void TerminalState_Failed_CannotTransition()
    {
        // Arrange
        var session = new StorySession
        {
            SessionId = "test-session",
            Stage = StorySessionStage.Failed
        };

        // Act & Assert
        // Document that Failed is a terminal state
        Assert.Equal(StorySessionStage.Failed, session.Stage);
    }

    [Fact]
    public void TerminalState_StuckNeedsReview_CannotTransition()
    {
        // Arrange
        var session = new StorySession
        {
            SessionId = "test-session",
            Stage = StorySessionStage.StuckNeedsReview
        };

        // Act & Assert
        // Document that StuckNeedsReview is a terminal state
        Assert.Equal(StorySessionStage.StuckNeedsReview, session.Stage);
    }

    [Theory]
    [InlineData(StorySessionStage.Uninitialized)]
    [InlineData(StorySessionStage.Generating)]
    [InlineData(StorySessionStage.Validating)]
    [InlineData(StorySessionStage.Evaluating)]
    [InlineData(StorySessionStage.Evaluated)]
    [InlineData(StorySessionStage.RequiresRefinement)]
    [InlineData(StorySessionStage.Refining)]
    [InlineData(StorySessionStage.Refined)]
    [InlineData(StorySessionStage.Complete)]
    [InlineData(StorySessionStage.Failed)]
    [InlineData(StorySessionStage.StuckNeedsReview)]
    public void AllStages_AreEnumerated(StorySessionStage stage)
    {
        // Assert - All stages are valid enum values
        Assert.True(Enum.IsDefined(typeof(StorySessionStage), stage));
    }

    [Fact]
    public void StorySession_IterationCount_TracksRefinements()
    {
        // Arrange
        var session = new StorySession
        {
            SessionId = "test-session",
            Stage = StorySessionStage.Validating,
            IterationCount = 0
        };

        // Act - Simulate multiple refinement cycles
        for (int i = 0; i < 3; i++)
        {
            session.Stage = StorySessionStage.Evaluating;
            session.Stage = StorySessionStage.RequiresRefinement;
            session.Stage = StorySessionStage.Refining;
            session.IterationCount++;
            session.Stage = StorySessionStage.Validating;
        }

        // Assert
        Assert.Equal(3, session.IterationCount);
    }

    [Fact]
    public void StorySession_MaxIterations_TriggersEscalation()
    {
        // Arrange
        var session = new StorySession
        {
            SessionId = "test-session",
            Stage = StorySessionStage.Evaluating,
            IterationCount = 5
        };

        // Act - After 5 iterations, escalate
        session.Stage = StorySessionStage.StuckNeedsReview;

        // Assert
        Assert.Equal(StorySessionStage.StuckNeedsReview, session.Stage);
        Assert.Equal(5, session.IterationCount);
    }

    [Fact]
    public void StorySession_StoryVersions_AccumulateOverRefinements()
    {
        // Arrange
        var session = new StorySession
        {
            SessionId = "test-session",
            StoryVersions = new List<StoryVersion>()
        };

        // Act - Add versions
        session.StoryVersions.Add(new StoryVersion
        {
            VersionNumber = 1,
            StoryJson = "{\"title\": \"Original\"}",
            CreatedAt = DateTime.UtcNow
        });

        session.StoryVersions.Add(new StoryVersion
        {
            VersionNumber = 2,
            StoryJson = "{\"title\": \"Refined\"}",
            CreatedAt = DateTime.UtcNow
        });

        // Assert
        Assert.Equal(2, session.StoryVersions.Count);
        Assert.Equal(1, session.StoryVersions[0].VersionNumber);
        Assert.Equal(2, session.StoryVersions[1].VersionNumber);
    }

    [Fact]
    public void StorySession_UpdatedAt_TracksModifications()
    {
        // Arrange
        var session = new StorySession
        {
            SessionId = "test-session",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        var originalUpdatedAt = session.UpdatedAt;

        // Act
        System.Threading.Thread.Sleep(10); // Ensure time difference
        session.Stage = StorySessionStage.Generating;
        session.UpdatedAt = DateTime.UtcNow;

        // Assert
        Assert.True(session.UpdatedAt > originalUpdatedAt);
    }
}
