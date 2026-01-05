using Microsoft.Extensions.Logging;
using Moq;
using Mystira.StoryGenerator.Application.Infrastructure.Agents;
using Mystira.StoryGenerator.Domain.Agents;

namespace Mystira.StoryGenerator.Infrastructure.Tests;

public class StorySessionRepositoryTests
{
    private readonly Mock<ILogger<CosmosStorySessionRepository>> _loggerMock;

    public StorySessionRepositoryTests()
    {
        _loggerMock = new Mock<ILogger<CosmosStorySessionRepository>>();
    }

    [Fact]
    public void StorySession_Creation_ShouldHaveDefaultValues()
    {
        // Arrange & Act
        var session = new StorySession();

        // Assert
        Assert.Equal(string.Empty, session.SessionId);
        Assert.Null(session.ThreadId);
        Assert.Equal(KnowledgeMode.FileSearch, session.KnowledgeMode);
        Assert.Equal(StorySessionStage.Generating, session.Stage);
        Assert.Equal(0, session.IterationCount);
        Assert.Equal(string.Empty, session.CurrentStoryVersion);
        Assert.NotNull(session.StoryVersions);
        Assert.Null(session.LastEvaluationReport);
        Assert.Null(session.UserFocus);
        Assert.Equal(default, session.CreatedAt);
        Assert.Equal(default, session.UpdatedAt);
        Assert.Equal(0m, session.CostEstimate);
    }

    [Fact]
    public void StorySession_WithValues_ShouldRetainValues()
    {
        // Arrange & Act
        var session = new StorySession
        {
            SessionId = "session-123",
            ThreadId = "thread-456",
            KnowledgeMode = KnowledgeMode.AISearch,
            Stage = StorySessionStage.Validating,
            IterationCount = 2,
            CurrentStoryVersion = "{\"title\": \"Test Story\"}",
            CostEstimate = 0.05m
        };

        // Assert
        Assert.Equal("session-123", session.SessionId);
        Assert.Equal("thread-456", session.ThreadId);
        Assert.Equal(KnowledgeMode.AISearch, session.KnowledgeMode);
        Assert.Equal(StorySessionStage.Validating, session.Stage);
        Assert.Equal(2, session.IterationCount);
        Assert.Equal("{\"title\": \"Test Story\"}", session.CurrentStoryVersion);
        Assert.Equal(0.05m, session.CostEstimate);
    }

    [Fact]
    public void StoryVersionSnapshot_ShouldHaveDefaultValues()
    {
        // Arrange & Act
        var snapshot = new StoryVersionSnapshot();

        // Assert
        Assert.Equal(0, snapshot.VersionNumber);
        Assert.Equal(string.Empty, snapshot.StoryJson);
        Assert.Equal(default, snapshot.CreatedAt);
        Assert.Equal(string.Empty, snapshot.StageWhenCreated);
        Assert.Equal(0, snapshot.IterationNumber);
    }

    [Fact]
    public void StoryVersionSnapshot_WithValues_ShouldRetainValues()
    {
        // Arrange & Act
        var snapshot = new StoryVersionSnapshot
        {
            VersionNumber = 1,
            StoryJson = "{\"title\": \"Story V1\"}",
            StageWhenCreated = "Generating",
            IterationNumber = 1
        };

        // Assert
        Assert.Equal(1, snapshot.VersionNumber);
        Assert.Equal("{\"title\": \"Story V1\"}", snapshot.StoryJson);
        Assert.Equal("Generating", snapshot.StageWhenCreated);
        Assert.Equal(1, snapshot.IterationNumber);
    }

    [Fact]
    public void EvaluationReport_ShouldHaveDefaultValues()
    {
        // Arrange & Act
        var report = new EvaluationReport();

        // Assert
        Assert.Equal(0, report.IterationNumber);
        Assert.Equal(default, report.EvaluationTimestamp);
        Assert.Equal(EvaluationStatus.Pass, report.OverallStatus);
        Assert.False(report.SafetyGatePassed);
        Assert.Equal(0f, report.AxesAlignmentScore);
        Assert.Equal(0f, report.DevPrinciplesScore);
        Assert.Equal(0f, report.NarrativeLogicScore);
        Assert.NotNull(report.Findings);
        Assert.Equal(string.Empty, report.Recommendation);
        Assert.Equal(0, report.TokenUsage);
    }

    [Fact]
    public void EvaluationReport_WithValues_ShouldRetainValues()
    {
        // Arrange & Act
        var report = new EvaluationReport
        {
            IterationNumber = 1,
            OverallStatus = EvaluationStatus.ReviewRequired,
            SafetyGatePassed = true,
            AxesAlignmentScore = 0.85f,
            DevPrinciplesScore = 0.92f,
            NarrativeLogicScore = 0.78f,
            Recommendation = "Consider improving character consistency"
        };

        // Assert
        Assert.Equal(1, report.IterationNumber);
        Assert.Equal(EvaluationStatus.ReviewRequired, report.OverallStatus);
        Assert.True(report.SafetyGatePassed);
        Assert.Equal(0.85f, report.AxesAlignmentScore);
        Assert.Equal(0.92f, report.DevPrinciplesScore);
        Assert.Equal(0.78f, report.NarrativeLogicScore);
        Assert.Equal("Consider improving character consistency", report.Recommendation);
    }

    [Fact]
    public void UserRefinementFocus_ShouldHaveDefaultValues()
    {
        // Arrange & Act
        var focus = new UserRefinementFocus();

        // Assert
        Assert.NotNull(focus.TargetSceneIds);
        Assert.Empty(focus.TargetSceneIds);
        Assert.NotNull(focus.Aspects);
        Assert.Empty(focus.Aspects);
        Assert.Equal(string.Empty, focus.Constraints);
        Assert.False(focus.IsFullRewrite);
    }

    [Fact]
    public void UserRefinementFocus_WithValues_ShouldRetainValues()
    {
        // Arrange & Act
        var focus = new UserRefinementFocus
        {
            TargetSceneIds = new List<string> { "scene-1", "scene-3" },
            Aspects = new List<string> { "dialogue", "pacing" },
            Constraints = "Keep the ending unchanged",
            IsFullRewrite = false
        };

        // Assert
        Assert.Equal(2, focus.TargetSceneIds.Count);
        Assert.Contains("scene-1", focus.TargetSceneIds);
        Assert.Equal(2, focus.Aspects.Count);
        Assert.Contains("dialogue", focus.Aspects);
        Assert.Equal("Keep the ending unchanged", focus.Constraints);
        Assert.False(focus.IsFullRewrite);
    }

    [Fact]
    public void IterationRecord_ShouldHaveDefaultValues()
    {
        // Arrange & Act
        var record = new IterationRecord();

        // Assert
        Assert.Equal(string.Empty, record.SessionId);
        Assert.Equal(0, record.IterationNumber);
        Assert.Equal(string.Empty, record.WriterRunId);
        Assert.Equal(string.Empty, record.JudgeRunId);
        Assert.Null(record.RefinerRunId);
        Assert.Null(record.RubricSummaryRunId);
        Assert.NotNull(record.StageDurations);
        Assert.NotNull(record.TokensByStage);
        Assert.Equal(0m, record.EstimatedCost);
        Assert.NotNull(record.EvaluationReport);
        Assert.Null(record.UserFeedback);
    }

    [Fact]
    public void IterationRecord_WithValues_ShouldRetainValues()
    {
        // Arrange & Act
        var record = new IterationRecord
        {
            SessionId = "session-123",
            IterationNumber = 2,
            WriterRunId = "run-writer-001",
            JudgeRunId = "run-judge-001",
            RefinerRunId = "run-refiner-001",
            EstimatedCost = 0.03m,
            UserFeedback = "More dialogue needed"
        };

        // Assert
        Assert.Equal("session-123", record.SessionId);
        Assert.Equal(2, record.IterationNumber);
        Assert.Equal("run-writer-001", record.WriterRunId);
        Assert.Equal("run-judge-001", record.JudgeRunId);
        Assert.Equal("run-refiner-001", record.RefinerRunId);
        Assert.Equal(0.03m, record.EstimatedCost);
        Assert.Equal("More dialogue needed", record.UserFeedback);
    }

    [Fact]
    public void KnowledgeMode_Enum_ShouldHaveExpectedValues()
    {
        // Assert
        Assert.Equal(0, (int)KnowledgeMode.FileSearch);
        Assert.Equal(1, (int)KnowledgeMode.AISearch);
    }

    [Fact]
    public void StorySessionStage_Enum_ShouldHaveExpectedValues()
    {
        // Assert
        Assert.Equal(0, (int)StorySessionStage.Generating);
        Assert.Equal(1, (int)StorySessionStage.Validating);
        Assert.Equal(2, (int)StorySessionStage.Evaluating);
        Assert.Equal(3, (int)StorySessionStage.RefinementRequested);
        Assert.Equal(4, (int)StorySessionStage.Refined);
        Assert.Equal(5, (int)StorySessionStage.Complete);
    }

    [Fact]
    public void EvaluationStatus_Enum_ShouldHaveExpectedValues()
    {
        // Assert
        Assert.Equal(0, (int)EvaluationStatus.Pass);
        Assert.Equal(1, (int)EvaluationStatus.Fail);
        Assert.Equal(2, (int)EvaluationStatus.ReviewRequired);
    }
}
