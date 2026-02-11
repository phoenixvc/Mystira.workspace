using Bunit;
using FluentAssertions;
using Mystira.App.PWA.Components;
using Xunit;

namespace Mystira.App.PWA.Tests.Components;

public class CoppaCompliancePillTests : BunitContext
{
    [Fact]
    public void CoppaCompliancePill_InitialState_TooltipNotVisible()
    {
        var cut = Render<CoppaCompliancePill>();

        cut.Find(".coppa-pill-container").Should().NotBeNull();
        cut.FindAll(".coppa-tooltip").Should().BeEmpty();
    }

    [Fact]
    public void CoppaCompliancePill_OnMouseEnter_ShowsTooltip()
    {
        var cut = Render<CoppaCompliancePill>();

        cut.Find(".coppa-pill-container").MouseOver();

        cut.Find(".coppa-tooltip").Should().NotBeNull();
        cut.Find(".coppa-tooltip-header").TextContent.Should().Contain("COPPA Compliance");
    }

    [Fact]
    public void CoppaCompliancePill_OnMouseLeave_HidesTooltip()
    {
        var cut = Render<CoppaCompliancePill>();

        // Show tooltip
        cut.Find(".coppa-pill-container").MouseOver();
        cut.FindAll(".coppa-tooltip").Should().HaveCount(1);

        // Hide tooltip
        cut.Find(".coppa-pill-container").MouseOut();
        cut.FindAll(".coppa-tooltip").Should().BeEmpty();
    }

    [Fact]
    public void CoppaCompliancePill_Tooltip_HasWarningMessage()
    {
        var cut = Render<CoppaCompliancePill>();
        cut.Find(".coppa-pill-container").MouseOver();

        cut.Find(".coppa-tooltip-warning").TextContent
            .Should().Contain("Do not collect children's data");
    }

    [Fact]
    public void CoppaCompliancePill_Tooltip_HasImplementationStatusLink()
    {
        var cut = Render<CoppaCompliancePill>();
        cut.Find(".coppa-pill-container").MouseOver();

        var links = cut.FindAll(".coppa-tooltip-link");
        links.Should().HaveCount(2);
        links[0].GetAttribute("href").Should().Be("/coppa-status");
    }

    [Fact]
    public void CoppaCompliancePill_Tooltip_FtcLinkOpensInNewTab()
    {
        var cut = Render<CoppaCompliancePill>();
        cut.Find(".coppa-pill-container").MouseOver();

        var ftcLink = cut.FindAll(".coppa-tooltip-link")[1];
        ftcLink.GetAttribute("target").Should().Be("_blank");
        ftcLink.GetAttribute("rel").Should().Contain("noopener");
    }
}
