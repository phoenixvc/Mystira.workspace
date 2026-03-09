namespace Mystira.StoryGenerator.Web.Models;

public sealed record YamlSyntaxColorScheme
{
    public string BackgroundColor { get; init; } = "#111827";
    public string ForegroundColor { get; init; } = "#E5E7EB";
    public string KeyColor { get; init; } = "#93C5FD";
    public string ValueColor { get; init; } = "#E5E7EB";
    public string StringColor { get; init; } = "#FDE68A";
    public string NumberColor { get; init; } = "#FCD34D";
    public string BooleanColor { get; init; } = "#38BDF8";
    public string NullColor { get; init; } = "#FCA5A5";
    public string CommentColor { get; init; } = "#9CA3AF";
    public string IndicatorColor { get; init; } = "#A855F7";
    public string AnchorColor { get; init; } = "#34D399";
    public string TagColor { get; init; } = "#F472B6";
    public string SeparatorColor { get; init; } = "#FBBF24";
    public string BorderColor { get; init; } = "rgba(148, 163, 184, 0.28)";
    public string HighlightBackgroundColor { get; init; } = "rgba(250, 204, 21, 0.35)";
    public string HighlightForegroundColor { get; init; } = "#111827";
    public string HighlightActiveBackgroundColor { get; init; } = "rgba(234, 179, 8, 0.55)";
    public string HighlightActiveForegroundColor { get; init; } = "#111827";
    public string HighlightActiveBorderColor { get; init; } = "rgba(202, 138, 4, 0.5)";

    public static YamlSyntaxColorScheme Default { get; } = new();
}
