using Bunit;
using FluentAssertions;
using Mystira.App.PWA.Components;
using Xunit;

namespace Mystira.App.PWA.Tests.Components;

public class SkeletonLoaderTests : BunitContext
{
    [Fact]
    public void SkeletonLoader_WithDefaults_RendersSixCards()
    {
        var cut = Render<SkeletonLoader>();

        cut.FindAll(".skeleton-card").Should().HaveCount(6);
    }

    [Theory]
    [InlineData(1)]
    [InlineData(3)]
    [InlineData(8)]
    [InlineData(12)]
    public void SkeletonLoader_WithCustomCount_RendersCorrectNumber(int count)
    {
        var cut = Render<SkeletonLoader>(parameters => parameters
            .Add(p => p.Count, count));

        cut.FindAll(".skeleton-card").Should().HaveCount(count);
    }

    [Fact]
    public void SkeletonLoader_EachCard_HasImageAndTextSkeletons()
    {
        var cut = Render<SkeletonLoader>(parameters => parameters
            .Add(p => p.Count, 1));

        var card = cut.Find(".skeleton-card");
        card.QuerySelectorAll(".skeleton-image").Length.Should().Be(1);
        card.QuerySelectorAll(".skeleton-text").Length.Should().BeGreaterThan(0);
        card.QuerySelectorAll(".skeleton-badge").Length.Should().Be(2);
    }

    [Fact]
    public void SkeletonLoader_UsesResponsiveGrid()
    {
        var cut = Render<SkeletonLoader>(parameters => parameters
            .Add(p => p.Count, 1));

        var col = cut.Find(".col-12");
        col.ClassName.Should().Contain("col-sm-6");
        col.ClassName.Should().Contain("col-md-4");
        col.ClassName.Should().Contain("col-lg-3");
    }
}
