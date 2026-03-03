using System.ComponentModel.DataAnnotations;

namespace Mystira.Contracts.StoryGenerator.Configuration;

public class LlmRateLimitOptions
{
    public const string SectionName = "LlmRateLimits";

    [Range(1, int.MaxValue)]
    public int PrefixSummaryRequestsPerMinute { get; set; } = 50;

    [Range(1, int.MaxValue)]
    public int SrlRequestsPerMinute { get; set; } = 250;
}
