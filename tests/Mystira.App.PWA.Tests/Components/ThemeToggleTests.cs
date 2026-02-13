using Bunit;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.JSInterop;
using Moq;
using Mystira.App.PWA.Components;
using Xunit;

namespace Mystira.App.PWA.Tests.Components;

public class ThemeToggleTests : BunitContext
{
    private readonly Mock<IJSRuntime> _jsRuntime;

    public ThemeToggleTests()
    {
        _jsRuntime = new Mock<IJSRuntime>();

        // Default: no saved theme, system preference is light
        _jsRuntime.Setup(js => js.InvokeAsync<string>("localStorage.getItem", It.IsAny<object[]>()))
            .ReturnsAsync(string.Empty);
        _jsRuntime.Setup(js => js.InvokeAsync<bool>("eval", It.IsAny<object[]>()))
            .ReturnsAsync(false);

        Services.AddSingleton<IJSRuntime>(_jsRuntime.Object);
    }

    [Fact]
    public void ThemeToggle_InitialRender_HasToggleButton()
    {
        var cut = Render<ThemeToggle>();

        cut.Find(".theme-toggle").Should().NotBeNull();
    }

    [Fact]
    public void ThemeToggle_InitialLightMode_ShowsMoonIcon()
    {
        var cut = Render<ThemeToggle>();

        cut.Find(".theme-toggle i").ClassName.Should().Contain("fa-moon");
    }

    [Fact]
    public void ThemeToggle_HasAccessibleLabel()
    {
        var cut = Render<ThemeToggle>();

        var button = cut.Find(".theme-toggle");
        button.GetAttribute("aria-label").Should().NotBeNullOrEmpty();
    }

    [Fact]
    public void ThemeToggle_OnClick_TogglesTheme()
    {
        var cut = Render<ThemeToggle>();

        cut.Find(".theme-toggle").Click();

        // After click, should switch to dark mode and show sun icon
        cut.Find(".theme-toggle i").ClassName.Should().Contain("fa-sun");
    }

    [Fact]
    public void ThemeToggle_OnClick_PersistsToLocalStorage()
    {
        var cut = Render<ThemeToggle>();

        cut.Find(".theme-toggle").Click();

        _jsRuntime.Verify(js => js.InvokeAsync<IJSVoidResult>(
            "localStorage.setItem",
            It.Is<object[]>(args => args.Length == 2 && (string)args[0] == "theme" && (string)args[1] == "dark")),
            Times.AtLeastOnce);
    }
}
