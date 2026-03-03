namespace Mystira.StoryGenerator.Application.Services.Prompting;

public interface IProjectGuidelinesService
{
    string GetForAgeGroup(string ageGroup);
    string GetDevelopmentPrinciples(string ageGroup);
    string GetSafetyGuidelines(string ageGroup);
}
