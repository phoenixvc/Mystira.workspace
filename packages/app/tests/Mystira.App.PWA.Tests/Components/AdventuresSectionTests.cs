using Bunit;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using Mystira.App.PWA.Components;
using Mystira.App.PWA.Models;
using Mystira.App.PWA.Services;
using Xunit;

namespace Mystira.App.PWA.Tests.Components;

public class AdventuresSectionTests : BunitContext
{
    private readonly Mock<IAuthService> _mockAuthService = new();
    private readonly Mock<IProfileService> _mockProfileService = new();
    private readonly Mock<IGameSessionService> _mockGameSessionService = new();
    private readonly Mock<IApiClient> _mockApiClient = new();

    private void RegisterServices()
    {
        Services.AddSingleton(_mockAuthService.Object);
        Services.AddSingleton(_mockProfileService.Object);
        Services.AddSingleton(_mockGameSessionService.Object);
        Services.AddSingleton(_mockApiClient.Object);
        Services.AddSingleton(Mock.Of<ILogger<AdventuresSection>>());
    }

    private void SetupUnauthenticated()
    {
        _mockAuthService.Setup(a => a.IsAuthenticatedAsync()).ReturnsAsync(false);
        _mockApiClient.Setup(a => a.GetBundlesAsync()).ReturnsAsync(new List<ContentBundle>());
        _mockApiClient.Setup(a => a.GetScenariosAsync()).ReturnsAsync(new List<Scenario>());
    }

    private void SetupAuthenticated()
    {
        var account = new Account { Id = "test-account-id" };
        _mockAuthService.Setup(a => a.IsAuthenticatedAsync()).ReturnsAsync(true);
        _mockAuthService.Setup(a => a.GetCurrentAccountAsync()).ReturnsAsync(account);
        _mockApiClient.Setup(a => a.GetBundlesAsync()).ReturnsAsync(new List<ContentBundle>());
        _mockApiClient.Setup(a => a.GetScenariosAsync()).ReturnsAsync(new List<Scenario>());
        _mockApiClient.Setup(a => a.GetSessionsByAccountAsync(It.IsAny<string>())).ReturnsAsync(new List<GameSession>());
        _mockApiClient.Setup(a => a.GetInProgressSessionsAsync(It.IsAny<string>())).ReturnsAsync(new List<GameSession>());
        _mockApiClient.Setup(a => a.GetScenariosWithGameStateAsync(It.IsAny<string>())).ReturnsAsync((ScenarioGameStateResponse?)null);
        _mockProfileService.Setup(p => p.HasProfilesAsync(It.IsAny<string>())).ReturnsAsync(true);
    }

    [Fact]
    public void AdventuresSection_WhenNotAuthenticated_DoesNotShowLoadingIndicator()
    {
        RegisterServices();
        SetupUnauthenticated();

        var cut = Render<AdventuresSection>();

        cut.FindAll(".loading-indicator-container").Should().BeEmpty();
        cut.FindAll(".loading-spinner").Should().BeEmpty();
    }

    [Fact]
    public void AdventuresSection_WhenNotAuthenticated_DoesNotShowFilterSection()
    {
        RegisterServices();
        SetupUnauthenticated();

        var cut = Render<AdventuresSection>();

        // Filter section and bundles grid should not appear for unauthenticated users
        cut.FindAll(".section-header").Should().BeEmpty();
    }

    [Fact]
    public void AdventuresSection_WhenAuthenticated_ShowsContent()
    {
        RegisterServices();
        SetupAuthenticated();

        var cut = Render<AdventuresSection>();

        // After loading completes, the authenticated user should see the bundles section
        cut.WaitForState(() => cut.FindAll(".section-title").Count > 0, TimeSpan.FromSeconds(2));
        cut.FindAll(".section-title").Should().NotBeEmpty();
    }
}
