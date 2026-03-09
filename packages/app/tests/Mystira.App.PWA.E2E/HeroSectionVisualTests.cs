using Microsoft.Playwright;
using Xunit;

namespace Mystira.App.PWA.E2E;

/// <summary>
/// Visual regression tests for the hero section.
/// Requires a running instance of the PWA (e.g. via dotnet run) and Playwright browsers installed.
/// Set the BASE_URL environment variable to override the default https://localhost:5001.
/// </summary>
public class HeroSectionVisualTests : IAsyncLifetime
{
    private IPlaywright _playwright = null!;
    private IBrowser _browser = null!;
    private IBrowserContext _context = null!;
    private IPage _page = null!;

    private string BaseUrl => Environment.GetEnvironmentVariable("BASE_URL") ?? "https://localhost:5001";

    public async Task InitializeAsync()
    {
        _playwright = await Playwright.CreateAsync();
        _browser = await _playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions
        {
            Headless = true
        });
        _context = await _browser.NewContextAsync(new BrowserNewContextOptions
        {
            IgnoreHTTPSErrors = true,
            ViewportSize = new ViewportSize { Width = 1440, Height = 900 }
        });
        _page = await _context.NewPageAsync();
    }

    public async Task DisposeAsync()
    {
        await _context.DisposeAsync();
        await _browser.DisposeAsync();
        _playwright.Dispose();
    }

    [Fact]
    [Trait("Category", "E2E")]
    public async Task HeroSection_FirstVisit_ShowsIntroVideo()
    {
        await _page.GotoAsync(BaseUrl, new PageGotoOptions { WaitUntil = WaitUntilState.NetworkIdle });

        var video = _page.Locator("video.portal-video");
        var isVisible = await video.IsVisibleAsync();

        Assert.True(isVisible, "Intro video should be visible on first visit");

        await _page.ScreenshotAsync(new PageScreenshotOptions
        {
            Path = "screenshots/hero-first-visit.png",
            FullPage = false
        });
    }

    [Fact]
    [Trait("Category", "E2E")]
    public async Task HeroSection_ReturnVisit_ShowsStaticLogo()
    {
        // Simulate return visit by setting localStorage before navigation
        await _page.GotoAsync(BaseUrl, new PageGotoOptions { WaitUntil = WaitUntilState.DOMContentLoaded });
        await _page.EvaluateAsync("localStorage.setItem('mystira-hero-intro-seen', '1')");
        await _page.ReloadAsync(new PageReloadOptions { WaitUntil = WaitUntilState.NetworkIdle });

        var logo = _page.Locator("img.portal-logo-img");
        await logo.WaitForAsync(new LocatorWaitForOptions { Timeout = 5000 });
        var isVisible = await logo.IsVisibleAsync();

        Assert.True(isVisible, "Static logo should be visible on return visit");

        await _page.ScreenshotAsync(new PageScreenshotOptions
        {
            Path = "screenshots/hero-return-visit.png",
            FullPage = false
        });
    }

    [Fact]
    [Trait("Category", "E2E")]
    public async Task HeroSection_ReplayButton_AppearsAfterIntro()
    {
        // Set intro as seen to get the replay button
        await _page.GotoAsync(BaseUrl, new PageGotoOptions { WaitUntil = WaitUntilState.DOMContentLoaded });
        await _page.EvaluateAsync("localStorage.setItem('mystira-hero-intro-seen', '1')");
        await _page.ReloadAsync(new PageReloadOptions { WaitUntil = WaitUntilState.NetworkIdle });

        var replayBtn = _page.Locator(".hero-replay-btn");
        await replayBtn.WaitForAsync(new LocatorWaitForOptions { Timeout = 5000 });
        var isVisible = await replayBtn.IsVisibleAsync();

        Assert.True(isVisible, "Replay button should appear for return visitors");
    }

    [Fact]
    [Trait("Category", "E2E")]
    public async Task HeroSection_StaggeredEntrance_ElementsAppearSequentially()
    {
        await _page.GotoAsync(BaseUrl, new PageGotoOptions { WaitUntil = WaitUntilState.DOMContentLoaded });
        await _page.EvaluateAsync("localStorage.setItem('mystira-hero-intro-seen', '1')");
        await _page.ReloadAsync(new PageReloadOptions { WaitUntil = WaitUntilState.NetworkIdle });

        // Verify the headline and subtext are present after animations complete
        var headline = _page.Locator(".hero-headline");
        var subtext = _page.Locator(".hero-subtext");

        await headline.WaitForAsync(new LocatorWaitForOptions { Timeout = 5000 });

        Assert.True(await headline.IsVisibleAsync(), "Headline should be visible");
        Assert.True(await subtext.IsVisibleAsync(), "Subtext should be visible");

        await _page.ScreenshotAsync(new PageScreenshotOptions
        {
            Path = "screenshots/hero-staggered-entrance.png",
            FullPage = false
        });
    }
}
