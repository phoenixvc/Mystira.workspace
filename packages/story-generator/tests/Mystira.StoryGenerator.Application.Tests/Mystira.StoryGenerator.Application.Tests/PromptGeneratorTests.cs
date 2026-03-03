using Moq;
using Mystira.StoryGenerator.Application.Infrastructure.Agents;
using Mystira.StoryGenerator.Application.Services.Prompting;
using Mystira.StoryGenerator.Contracts.Agents;
using Mystira.StoryGenerator.Domain.Agents;

namespace Mystira.StoryGenerator.Application.Tests;

/// <summary>
/// Tests for PromptGenerator, covering age group extraction from story JSON.
/// Regression tests for bug where "3-5" age group was overridden to "6-9".
/// </summary>
public class PromptGeneratorTests
{
    private const string StoryJsonWithAge35 = """
        {
          "title": "Test Story",
          "description": "A test story",
          "image": "img_001",
          "tags": ["adventure"],
          "difficulty": "Easy",
          "session_length": "Short",
          "age_group": "3-5",
          "minimum_age": 3,
          "core_axes": ["kindness"],
          "archetypes": ["the_hero"],
          "characters": [],
          "scenes": []
        }
        """;

    private const string StoryJsonWithAge69 = """
        {
          "title": "Test Story",
          "description": "A test story",
          "image": "img_001",
          "tags": ["adventure"],
          "difficulty": "Medium",
          "session_length": "Medium",
          "age_group": "6-9",
          "minimum_age": 6,
          "core_axes": ["courage"],
          "archetypes": ["the_explorer"],
          "characters": [],
          "scenes": []
        }
        """;

    private static EvaluationReport MakeEvaluationReport() => new()
    {
        IterationNumber = 1,
        OverallStatus = EvaluationStatus.Fail,
        SafetyGatePassed = true,
        AxesAlignmentScore = 0.5f,
        DevPrinciplesScore = 0.5f,
        NarrativeLogicScore = 0.5f,
        Findings = new Dictionary<string, List<string>>(),
        Recommendation = "Improve something"
    };

    [Fact]
    public void GenerateRefinerPrompt_ExtractsAgeGroupFromRootLevel_NotMetadata()
    {
        // Arrange
        var guidelinesMock = new Mock<IProjectGuidelinesService>();
        guidelinesMock.Setup(g => g.GetForAgeGroup(It.IsAny<string>())).Returns("guidelines");
        guidelinesMock.Setup(g => g.GetDevelopmentPrinciples(It.IsAny<string>())).Returns("principles");
        guidelinesMock.Setup(g => g.GetSafetyGuidelines(It.IsAny<string>())).Returns("safety");

        var knowledgeMock = new Mock<IKnowledgeProvider>();
        knowledgeMock.Setup(k => k.GetContextualGuidance(It.IsAny<string?>())).Returns("knowledge");

        var generator = new PromptGenerator(guidelinesMock.Object, knowledgeMock.Object);
        var focus = new UserRefinementFocus { Constraints = "improve it", IsFullRewrite = false };

        // Act
        generator.GenerateRefinerPrompt(StoryJsonWithAge35, MakeEvaluationReport(), focus);

        // Assert: knowledge provider must be called with "3-5", NOT the "6-9" fallback
        knowledgeMock.Verify(k => k.GetContextualGuidance("3-5"), Times.Once);
        knowledgeMock.Verify(k => k.GetContextualGuidance("6-9"), Times.Never);
    }

    [Fact]
    public void GenerateRubricPrompt_ExtractsAgeGroupFromRootLevel_NotMetadata()
    {
        // Arrange
        var guidelinesMock = new Mock<IProjectGuidelinesService>();
        guidelinesMock.Setup(g => g.GetForAgeGroup(It.IsAny<string>())).Returns("guidelines");

        var knowledgeMock = new Mock<IKnowledgeProvider>();
        knowledgeMock.Setup(k => k.GetContextualGuidance(It.IsAny<string?>())).Returns("knowledge");

        var generator = new PromptGenerator(guidelinesMock.Object, knowledgeMock.Object);

        // Act
        generator.GenerateRubricPrompt(StoryJsonWithAge35, MakeEvaluationReport(), iteration: 1);

        // Assert: knowledge provider must be called with "3-5", NOT the "6-9" fallback
        knowledgeMock.Verify(k => k.GetContextualGuidance("3-5"), Times.Once);
        knowledgeMock.Verify(k => k.GetContextualGuidance("6-9"), Times.Never);
    }

    [Fact]
    public void GenerateRefinerPrompt_FallsBackTo69_WhenAgeGroupMissing()
    {
        // Arrange: story JSON with no age_group field
        const string storyJsonNoAgeGroup = """
            {
              "title": "Story Without Age Group",
              "description": "No age field",
              "scenes": []
            }
            """;

        var guidelinesMock = new Mock<IProjectGuidelinesService>();
        guidelinesMock.Setup(g => g.GetForAgeGroup(It.IsAny<string>())).Returns("guidelines");

        var knowledgeMock = new Mock<IKnowledgeProvider>();
        knowledgeMock.Setup(k => k.GetContextualGuidance(It.IsAny<string?>())).Returns("knowledge");

        var generator = new PromptGenerator(guidelinesMock.Object, knowledgeMock.Object);
        var focus = new UserRefinementFocus { Constraints = "improve it", IsFullRewrite = false };

        // Act
        generator.GenerateRefinerPrompt(storyJsonNoAgeGroup, MakeEvaluationReport(), focus);

        // Assert: falls back to "6-9" when field is absent
        knowledgeMock.Verify(k => k.GetContextualGuidance("6-9"), Times.Once);
    }

    [Fact]
    public void GenerateRefinerPrompt_CorrectlyUses69_WhenStoryJsonContains69()
    {
        // Arrange: story with age_group "6-9" should still use "6-9"
        var guidelinesMock = new Mock<IProjectGuidelinesService>();
        guidelinesMock.Setup(g => g.GetForAgeGroup(It.IsAny<string>())).Returns("guidelines");

        var knowledgeMock = new Mock<IKnowledgeProvider>();
        knowledgeMock.Setup(k => k.GetContextualGuidance(It.IsAny<string?>())).Returns("knowledge");

        var generator = new PromptGenerator(guidelinesMock.Object, knowledgeMock.Object);
        var focus = new UserRefinementFocus { Constraints = "improve it", IsFullRewrite = false };

        // Act
        generator.GenerateRefinerPrompt(StoryJsonWithAge69, MakeEvaluationReport(), focus);

        // Assert
        knowledgeMock.Verify(k => k.GetContextualGuidance("6-9"), Times.Once);
        knowledgeMock.Verify(k => k.GetContextualGuidance("3-5"), Times.Never);
    }
}
