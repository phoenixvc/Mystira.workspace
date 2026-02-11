using Bunit;
using FluentAssertions;
using Mystira.App.PWA.Components;
using Xunit;

namespace Mystira.App.PWA.Tests.Components;

public class EmptyStateTests : BunitContext
{
    [Fact]
    public void EmptyState_WithDefaults_RendersDefaultTitleAndMessage()
    {
        var cut = Render<EmptyState>();

        cut.Find(".empty-state-title").TextContent.Should().Be("No Results Found");
        cut.Find(".empty-state-message").TextContent.Should().Be("Try adjusting your search or filters");
    }

    [Fact]
    public void EmptyState_WithCustomTitle_RendersCustomTitle()
    {
        var cut = Render<EmptyState>(parameters => parameters
            .Add(p => p.Title, "No Adventures")
            .Add(p => p.Message, "Check back later for new adventures"));

        cut.Find(".empty-state-title").TextContent.Should().Be("No Adventures");
        cut.Find(".empty-state-message").TextContent.Should().Be("Check back later for new adventures");
    }

    [Fact]
    public void EmptyState_WithCustomIcon_RendersCorrectIconClass()
    {
        var cut = Render<EmptyState>(parameters => parameters
            .Add(p => p.IconClass, "fas fa-dragon fa-4x"));

        cut.Find(".empty-state-icon i").ClassName.Should().Contain("fa-dragon");
    }

    [Fact]
    public void EmptyState_WithClearButton_RendersClearButton()
    {
        var cleared = false;
        var cut = Render<EmptyState>(parameters => parameters
            .Add(p => p.ShowClearButton, true)
            .Add(p => p.OnClearFilters, () => cleared = true));

        var clearButton = cut.Find(".btn-clear-filters");
        clearButton.Should().NotBeNull();
        clearButton.TextContent.Should().Contain("Clear Filters");
    }

    [Fact]
    public void EmptyState_ClearButton_InvokesCallback()
    {
        var cleared = false;
        var cut = Render<EmptyState>(parameters => parameters
            .Add(p => p.ShowClearButton, true)
            .Add(p => p.OnClearFilters, () => cleared = true));

        cut.Find(".btn-clear-filters").Click();

        cleared.Should().BeTrue();
    }

    [Fact]
    public void EmptyState_WithRetryButton_RendersRetryButton()
    {
        var retried = false;
        var cut = Render<EmptyState>(parameters => parameters
            .Add(p => p.ShowRetryButton, true)
            .Add(p => p.OnRetry, () => retried = true));

        var retryButton = cut.Find(".btn-retry");
        retryButton.Should().NotBeNull();
        retryButton.TextContent.Should().Contain("Try Again");
    }

    [Fact]
    public void EmptyState_RetryButton_InvokesCallback()
    {
        var retried = false;
        var cut = Render<EmptyState>(parameters => parameters
            .Add(p => p.ShowRetryButton, true)
            .Add(p => p.OnRetry, () => retried = true));

        cut.Find(".btn-retry").Click();

        retried.Should().BeTrue();
    }

    [Fact]
    public void EmptyState_WithoutCallbacks_DoesNotRenderButtons()
    {
        var cut = Render<EmptyState>(parameters => parameters
            .Add(p => p.ShowClearButton, true)
            .Add(p => p.ShowRetryButton, true));

        // Buttons only render when both Show* is true AND delegate is assigned
        cut.FindAll(".btn-clear-filters").Should().BeEmpty();
        cut.FindAll(".btn-retry").Should().BeEmpty();
    }

    [Fact]
    public void EmptyState_WithChildContent_RendersActions()
    {
        var cut = Render<EmptyState>(parameters => parameters
            .AddChildContent("<button class='custom-action'>Custom Action</button>"));

        cut.Find(".empty-state-actions").Should().NotBeNull();
        cut.Find(".custom-action").TextContent.Should().Be("Custom Action");
    }
}
