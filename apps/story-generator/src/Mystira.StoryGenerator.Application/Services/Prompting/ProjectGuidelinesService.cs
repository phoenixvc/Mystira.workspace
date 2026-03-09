using System.Reflection;

namespace Mystira.StoryGenerator.Application.Services.Prompting;

public sealed class ProjectGuidelinesService : IProjectGuidelinesService
{
    private readonly Lazy<string> _projectGuidelines;
    private readonly Lazy<string> _developmentPrinciples;
    private readonly Lazy<string> _safetyGuidelines;

    public ProjectGuidelinesService()
    {
        _projectGuidelines = new Lazy<string>(() => ReadResourceOrFallback("Mystira.StoryGenerator.Application.Resources.FoundryProjectGuidelines.md"));
        _developmentPrinciples = new Lazy<string>(() => ReadResourceOrFallback("Mystira.StoryGenerator.Application.Resources.FoundryDevelopmentPrinciples.md"));
        _safetyGuidelines = new Lazy<string>(() => ReadResourceOrFallback("Mystira.StoryGenerator.Application.Resources.FoundrySafetyGuidelines.md"));
    }

    public string GetForAgeGroup(string ageGroup)
    {
        return $"(Age group: {ageGroup})\n\n{_projectGuidelines.Value}";
    }

    public string GetDevelopmentPrinciples(string ageGroup)
    {
        return $"(Age group: {ageGroup})\n\n{_developmentPrinciples.Value}";
    }

    public string GetSafetyGuidelines(string ageGroup)
    {
        return $"(Age group: {ageGroup})\n\n{_safetyGuidelines.Value}";
    }

    private static string ReadResourceOrFallback(string resourceName)
    {
        var assembly = Assembly.GetExecutingAssembly();
        using var stream = assembly.GetManifestResourceStream(resourceName);
        if (stream == null)
        {
            return $"(Missing embedded resource: {resourceName})";
        }

        using var reader = new StreamReader(stream);
        return reader.ReadToEnd();
    }
}
