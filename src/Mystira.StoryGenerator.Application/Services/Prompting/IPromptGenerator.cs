using Mystira.StoryGenerator.Domain.Agents;

namespace Mystira.StoryGenerator.Application.Services.Prompting;

public interface IPromptGenerator
{
    string GenerateWriterPrompt(string storyPrompt, string ageGroup, List<string> axes);
    string GenerateJudgePrompt(string storyJson, string ageGroup, List<string> axes);
    string GenerateRefinerPrompt(string storyJson, EvaluationReport report, UserRefinementFocus focus);
    string GenerateRubricPrompt(string storyJson, EvaluationReport report, int iteration);
}
