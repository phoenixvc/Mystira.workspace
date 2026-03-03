using Bunit;
using FluentAssertions;
using Mystira.App.PWA.Components;
using Xunit;

namespace Mystira.App.PWA.Tests.Components;

public class WarningModalTests : BunitContext
{
    [Fact]
    public void WarningModal_WhenNotVisible_RendersNothing()
    {
        var cut = Render<WarningModal>(parameters => parameters
            .Add(p => p.ShowModal, false));

        cut.FindAll(".modal").Should().BeEmpty();
        cut.FindAll(".modal-backdrop").Should().BeEmpty();
    }

    [Fact]
    public void WarningModal_WhenVisible_RendersModalWithBackdrop()
    {
        var cut = Render<WarningModal>(parameters => parameters
            .Add(p => p.ShowModal, true)
            .Add(p => p.Title, "Warning"));

        cut.Find(".modal-backdrop").Should().NotBeNull();
        cut.Find(".modal").Should().NotBeNull();
        cut.Find(".modal-title").TextContent.Should().Contain("Warning");
    }

    [Fact]
    public void WarningModal_HasAccessibilityAttributes()
    {
        var cut = Render<WarningModal>(parameters => parameters
            .Add(p => p.ShowModal, true)
            .Add(p => p.Title, "Test Modal"));

        var modal = cut.Find(".modal");
        modal.GetAttribute("role").Should().Be("dialog");
        modal.GetAttribute("aria-modal").Should().Be("true");
    }

    [Fact]
    public void WarningModal_WithCustomButtonText_RendersCorrectly()
    {
        var cut = Render<WarningModal>(parameters => parameters
            .Add(p => p.ShowModal, true)
            .Add(p => p.CancelText, "Nope")
            .Add(p => p.ContinueText, "Let's Go"));

        cut.Find(".btn-secondary").TextContent.Should().Contain("Nope");
        cut.Find(".btn-primary-custom").TextContent.Should().Contain("Let's Go");
    }

    [Fact]
    public void WarningModal_ContinueButton_InvokesCallback()
    {
        var continued = false;
        var cut = Render<WarningModal>(parameters => parameters
            .Add(p => p.ShowModal, true)
            .Add(p => p.OnContinue, () => continued = true));

        cut.Find(".btn-primary-custom").Click();

        continued.Should().BeTrue();
    }

    [Fact]
    public void WarningModal_CancelButton_InvokesCallback()
    {
        var cancelled = false;
        var cut = Render<WarningModal>(parameters => parameters
            .Add(p => p.ShowModal, true)
            .Add(p => p.OnCancel, () => cancelled = true));

        cut.Find(".btn-secondary").Click();

        cancelled.Should().BeTrue();
    }

    [Fact]
    public void WarningModal_CloseButton_InvokesCancel()
    {
        var cancelled = false;
        var cut = Render<WarningModal>(parameters => parameters
            .Add(p => p.ShowModal, true)
            .Add(p => p.OnCancel, () => cancelled = true));

        cut.Find(".btn-close").Click();

        cancelled.Should().BeTrue();
    }

    [Fact]
    public void WarningModal_DontShowAgainCheckbox_IsPresent()
    {
        var cut = Render<WarningModal>(parameters => parameters
            .Add(p => p.ShowModal, true));

        cut.Find(".form-check-input").Should().NotBeNull();
        cut.Find(".form-check-label").TextContent.Should().Contain("Don't show this message again");
    }

    [Fact]
    public void WarningModal_WithBodyContent_RendersContent()
    {
        var cut = Render<WarningModal>(parameters => parameters
            .Add(p => p.ShowModal, true)
            .Add(p => p.BodyContent, (Microsoft.AspNetCore.Components.RenderFragment)(builder =>
            {
                builder.AddContent(0, "Are you sure about this?");
            })));

        cut.Find(".modal-body").TextContent.Should().Contain("Are you sure about this?");
    }
}
