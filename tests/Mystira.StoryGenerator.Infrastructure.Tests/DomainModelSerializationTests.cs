using System.Text.Json;
using Mystira.StoryGenerator.Domain.Agents;

namespace Mystira.StoryGenerator.Infrastructure.Tests;

/// <summary>
/// Tests for JSON serialization of domain models.
/// </summary>
public class DomainModelSerializationTests
{
    private readonly JsonSerializerOptions _jsonOptions;

    public DomainModelSerializationTests()
    {
        _jsonOptions = new JsonSerializerOptions
        {
            WriteIndented = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };
    }

    [Fact]
    public void StorySession_Serialization_ShouldRoundTrip()
    {
        // Arrange
        var session = new StorySession
        {
            SessionId = "test-session-123",
            ThreadId = "thread-456",
            KnowledgeMode = KnowledgeMode.AISearch,
            Stage = StorySessionStage.Complete,
            IterationCount = 3,
            CurrentStoryVersion = "{\"title\": \"My Story\"}",
            CostEstimate = 0.15m,
            CreatedAt = new DateTime(2024, 1, 1, 12, 0, 0, DateTimeKind.Utc),
            UpdatedAt = new DateTime(2024, 1, 1, 12, 30, 0, DateTimeKind.Utc)
        };

        // Act
        var json = JsonSerializer.Serialize(session, _jsonOptions);
        var deserialized = JsonSerializer.Deserialize<StorySession>(json, _jsonOptions);

        // Assert
        Assert.NotNull(deserialized);
        Assert.Equal(session.SessionId, deserialized.SessionId);
        Assert.Equal(session.ThreadId, deserialized.ThreadId);
        Assert.Equal(session.KnowledgeMode, deserialized.KnowledgeMode);
        Assert.Equal(session.Stage, deserialized.Stage);
        Assert.Equal(session.IterationCount, deserialized.IterationCount);
        Assert.Equal(session.CurrentStoryVersion, deserialized.CurrentStoryVersion);
        Assert.Equal(session.CostEstimate, deserialized.CostEstimate);
    }

    [Fact]
    public void StoryVersionSnapshot_Serialization_ShouldRoundTrip()
    {
        // Arrange
        var snapshot = new StoryVersionSnapshot
        {
            VersionNumber = 2,
            StoryJson = "{\"title\": \"Version 2\"}",
            StageWhenCreated = "Evaluating",
            IterationNumber = 2,
            CreatedAt = new DateTime(2024, 1, 1, 12, 15, 0, DateTimeKind.Utc)
        };

        // Act
        var json = JsonSerializer.Serialize(snapshot, _jsonOptions);
        var deserialized = JsonSerializer.Deserialize<StoryVersionSnapshot>(json, _jsonOptions);

        // Assert
        Assert.NotNull(deserialized);
        Assert.Equal(snapshot.VersionNumber, deserialized.VersionNumber);
        Assert.Equal(snapshot.StoryJson, deserialized.StoryJson);
        Assert.Equal(snapshot.StageWhenCreated, deserialized.StageWhenCreated);
        Assert.Equal(snapshot.IterationNumber, deserialized.IterationNumber);
    }

    [Fact]
    public void EvaluationReport_Serialization_ShouldRoundTrip()
    {
        // Arrange
        var report = new EvaluationReport
        {
            IterationNumber = 1,
            OverallStatus = EvaluationStatus.Pass,
            SafetyGatePassed = true,
            AxesAlignmentScore = 0.92f,
            DevPrinciplesScore = 0.88f,
            NarrativeLogicScore = 0.95f,
            Recommendation = "Good story!",
            TokenUsage = 1500,
            EvaluationTimestamp = new DateTime(2024, 1, 1, 12, 5, 0, DateTimeKind.Utc),
            Findings = new Dictionary<string, List<string>>
            {
                { "safety", new List<string> { "No issues found" } },
                { "quality", new List<string> { "Well written", "Engaging" } }
            }
        };

        // Act
        var json = JsonSerializer.Serialize(report, _jsonOptions);
        var deserialized = JsonSerializer.Deserialize<EvaluationReport>(json, _jsonOptions);

        // Assert
        Assert.NotNull(deserialized);
        Assert.Equal(report.IterationNumber, deserialized.IterationNumber);
        Assert.Equal(report.OverallStatus, deserialized.OverallStatus);
        Assert.Equal(report.SafetyGatePassed, deserialized.SafetyGatePassed);
        Assert.Equal(report.AxesAlignmentScore, deserialized.AxesAlignmentScore);
        Assert.Equal(report.DevPrinciplesScore, deserialized.DevPrinciplesScore);
        Assert.Equal(report.NarrativeLogicScore, deserialized.NarrativeLogicScore);
        Assert.Equal(report.Recommendation, deserialized.Recommendation);
        Assert.Equal(report.TokenUsage, deserialized.TokenUsage);
        Assert.NotNull(deserialized.Findings);
        Assert.Equal(2, deserialized.Findings.Count);
    }

    [Fact]
    public void UserRefinementFocus_Serialization_ShouldRoundTrip()
    {
        // Arrange
        var focus = new UserRefinementFocus
        {
            TargetSceneIds = new List<string> { "scene-1", "scene-3", "scene-5" },
            Aspects = new List<string> { "dialogue", "pacing" },
            Constraints = "Make it funnier",
            IsFullRewrite = false
        };

        // Act
        var json = JsonSerializer.Serialize(focus, _jsonOptions);
        var deserialized = JsonSerializer.Deserialize<UserRefinementFocus>(json, _jsonOptions);

        // Assert
        Assert.NotNull(deserialized);
        Assert.Equal(3, deserialized.TargetSceneIds.Count);
        Assert.Contains("scene-1", deserialized.TargetSceneIds);
        Assert.Equal(2, deserialized.Aspects.Count);
        Assert.Contains("dialogue", deserialized.Aspects);
        Assert.Equal("Make it funnier", deserialized.Constraints);
        Assert.False(deserialized.IsFullRewrite);
    }

    [Fact]
    public void IterationRecord_Serialization_ShouldRoundTrip()
    {
        // Arrange
        var record = new IterationRecord
        {
            SessionId = "session-123",
            IterationNumber = 2,
            WriterRunId = "run-writer-001",
            JudgeRunId = "run-judge-001",
            RefinerRunId = "run-refiner-001",
            RubricSummaryRunId = "run-rubric-001",
            EstimatedCost = 0.05m,
            UserFeedback = "Add more description",
            StageDurations = new Dictionary<string, TimeSpan>
            {
                { "writer", TimeSpan.FromSeconds(30) },
                { "judge", TimeSpan.FromSeconds(45) },
                { "refiner", TimeSpan.FromSeconds(60) }
            },
            TokensByStage = new Dictionary<string, int>
            {
                { "writer", 5000 },
                { "judge", 3000 },
                { "refiner", 4000 }
            },
            EvaluationReport = new EvaluationReport
            {
                IterationNumber = 2,
                OverallStatus = EvaluationStatus.ReviewRequired,
                TokenUsage = 12000
            }
        };

        // Act
        var json = JsonSerializer.Serialize(record, _jsonOptions);
        var deserialized = JsonSerializer.Deserialize<IterationRecord>(json, _jsonOptions);

        // Assert
        Assert.NotNull(deserialized);
        Assert.Equal(record.SessionId, deserialized.SessionId);
        Assert.Equal(record.IterationNumber, deserialized.IterationNumber);
        Assert.Equal(record.WriterRunId, deserialized.WriterRunId);
        Assert.Equal(record.JudgeRunId, deserialized.JudgeRunId);
        Assert.Equal(record.RefinerRunId, deserialized.RefinerRunId);
        Assert.Equal(record.RubricSummaryRunId, deserialized.RubricSummaryRunId);
        Assert.Equal(record.EstimatedCost, deserialized.EstimatedCost);
        Assert.Equal(record.UserFeedback, deserialized.UserFeedback);
        Assert.Equal(3, deserialized.StageDurations.Count);
        Assert.Equal(3, deserialized.TokensByStage.Count);
        Assert.NotNull(deserialized.EvaluationReport);
    }

    [Fact]
    public void StorySession_WithNestedObjects_ShouldSerializeCorrectly()
    {
        // Arrange
        var session = new StorySession
        {
            SessionId = "session-123",
            KnowledgeMode = KnowledgeMode.FileSearch,
            Stage = StorySessionStage.Evaluating,
            IterationCount = 1,
            CurrentStoryVersion = "{\"title\": \"Test\"}",
            StoryVersions = new List<StoryVersionSnapshot>
            {
                new() { VersionNumber = 1, StoryJson = "{}", IterationNumber = 1, StageWhenCreated = "Generating" }
            },
            LastEvaluationReport = new EvaluationReport
            {
                IterationNumber = 1,
                OverallStatus = EvaluationStatus.Pass,
                TokenUsage = 1000
            },
            UserFocus = new UserRefinementFocus
            {
                TargetSceneIds = new List<string> { "scene-1" },
                Aspects = new List<string> { "voice" }
            }
        };

        // Act
        var json = JsonSerializer.Serialize(session, _jsonOptions);
        var deserialized = JsonSerializer.Deserialize<StorySession>(json, _jsonOptions);

        // Assert
        Assert.NotNull(deserialized);
        Assert.Single(deserialized.StoryVersions);
        Assert.NotNull(deserialized.LastEvaluationReport);
        Assert.NotNull(deserialized.UserFocus);
        Assert.Single(deserialized.UserFocus.TargetSceneIds);
    }

    [Fact]
    public void EnumValues_ShouldSerializeToString()
    {
        // Arrange
        var session = new StorySession
        {
            KnowledgeMode = KnowledgeMode.AISearch,
            Stage = StorySessionStage.Complete
        };

        // Act
        var json = JsonSerializer.Serialize(session, _jsonOptions);

        // Assert
        Assert.Contains("AISearch", json);
        Assert.Contains("Complete", json);
    }
}
