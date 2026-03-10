using Mystira.Shared.Exceptions;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Mystira.App.Application.Configuration.StoryProtocol;
using Mystira.App.Application.CQRS.Attribution.Queries;
using Mystira.App.Application.Ports.Data;
using Mystira.Domain.Enums;
using Mystira.Domain.Models;

namespace Mystira.App.Application.Tests.CQRS.Attribution;

public class AttributionQueryHandlerTests
{
    private readonly Mock<IContentBundleRepository> _bundleRepository;
    private readonly Mock<IScenarioRepository> _scenarioRepository;
    private readonly IOptions<StoryProtocolOptions> _options;

    public AttributionQueryHandlerTests()
    {
        _bundleRepository = new Mock<IContentBundleRepository>();
        _scenarioRepository = new Mock<IScenarioRepository>();
        _options = Options.Create(new StoryProtocolOptions { ExplorerBaseUrl = "https://explorer.test" });
    }

    #region GetBundleAttributionQueryHandler Tests

    [Fact]
    public async Task GetBundleAttribution_WithExistingBundle_ReturnsAttribution()
    {
        var bundle = new ContentBundle
        {
            Id = "bundle-1",
            Title = "Test Bundle",
            StoryProtocol = new ScenarioStoryProtocol
            {
                IpAssetId = "ip-1",
                Contributors = new List<Contributor>
                {
                    new() { Name = "Alice", Role = ContributorRole.Writer, ContributionPercentage = 60 },
                    new() { Name = "Bob", Role = ContributorRole.Artist, ContributionPercentage = 40 }
                }
            }
        };
        _bundleRepository.Setup(r => r.GetByIdAsync("bundle-1", It.IsAny<CancellationToken>()))
            .ReturnsAsync(bundle);

        var result = await GetBundleAttributionQueryHandler.Handle(
            new GetBundleAttributionQuery("bundle-1"), _bundleRepository.Object,
            Mock.Of<ILogger<GetBundleAttributionQuery>>(), CancellationToken.None);

        result.Should().NotBeNull();
        result!.ContentId.Should().Be("bundle-1");
        result.ContentTitle.Should().Be("Test Bundle");
        result.IsIpRegistered.Should().BeTrue();
        result.Credits.Should().HaveCount(2);
    }

    [Fact]
    public async Task GetBundleAttribution_WithNonExistingBundle_ReturnsNull()
    {
        _bundleRepository.Setup(r => r.GetByIdAsync("missing", It.IsAny<CancellationToken>()))
            .ReturnsAsync(default(ContentBundle));

        var result = await GetBundleAttributionQueryHandler.Handle(
            new GetBundleAttributionQuery("missing"), _bundleRepository.Object,
            Mock.Of<ILogger<GetBundleAttributionQuery>>(), CancellationToken.None);

        result.Should().BeNull();
    }

    [Fact]
    public async Task GetBundleAttribution_WithEmptyId_ThrowsValidationException()
    {
        var act = () => GetBundleAttributionQueryHandler.Handle(
            new GetBundleAttributionQuery(""), _bundleRepository.Object,
            Mock.Of<ILogger<GetBundleAttributionQuery>>(), CancellationToken.None);

        await act.Should().ThrowAsync<ValidationException>();
    }

    #endregion

    #region GetBundleIpStatusQueryHandler Tests

    [Fact]
    public async Task GetBundleIpStatus_WithRegisteredBundle_ReturnsStatusWithExplorerUrl()
    {
        var bundle = new ContentBundle
        {
            Id = "bundle-1",
            Title = "Test Bundle",
            StoryProtocol = new ScenarioStoryProtocol
            {
                IpAssetId = "ip-asset-123",
                RegisteredAt = DateTime.UtcNow,
                RegistrationTxHash = "0xabc",
                Contributors = new List<Contributor> { new() { Name = "Alice" } }
            }
        };
        _bundleRepository.Setup(r => r.GetByIdAsync("bundle-1", It.IsAny<CancellationToken>()))
            .ReturnsAsync(bundle);

        var result = await GetBundleIpStatusQueryHandler.Handle(
            new GetBundleIpStatusQuery("bundle-1"), _bundleRepository.Object,
            Mock.Of<ILogger<GetBundleIpStatusQuery>>(), _options, CancellationToken.None);

        result.Should().NotBeNull();
        result!.IsRegistered.Should().BeTrue();
        result.IpAssetId.Should().Be("ip-asset-123");
        result.ExplorerUrl.Should().Be("https://explorer.test/address/ip-asset-123");
        result.ContributorCount.Should().Be(1);
    }

    [Fact]
    public async Task GetBundleIpStatus_WithNonExistingBundle_ReturnsNull()
    {
        _bundleRepository.Setup(r => r.GetByIdAsync("missing", It.IsAny<CancellationToken>()))
            .ReturnsAsync(default(ContentBundle));

        var result = await GetBundleIpStatusQueryHandler.Handle(
            new GetBundleIpStatusQuery("missing"), _bundleRepository.Object,
            Mock.Of<ILogger<GetBundleIpStatusQuery>>(), _options, CancellationToken.None);

        result.Should().BeNull();
    }

    [Fact]
    public async Task GetBundleIpStatus_WithEmptyId_ThrowsValidationException()
    {
        var act = () => GetBundleIpStatusQueryHandler.Handle(
            new GetBundleIpStatusQuery(""), _bundleRepository.Object,
            Mock.Of<ILogger<GetBundleIpStatusQuery>>(), _options, CancellationToken.None);

        await act.Should().ThrowAsync<ValidationException>();
    }

    #endregion

    #region GetScenarioAttributionQueryHandler Tests

    [Fact]
    public async Task GetScenarioAttribution_WithExistingScenario_ReturnsAttribution()
    {
        var scenario = new Scenario
        {
            Id = "scenario-1",
            Title = "Test Scenario",
            StoryProtocol = new ScenarioStoryProtocol
            {
                IpAssetId = "ip-1",
                Contributors = new List<Contributor>
                {
                    new() { Name = "Charlie", Role = ContributorRole.GameDesigner }
                }
            }
        };
        _scenarioRepository.Setup(r => r.GetByIdAsync("scenario-1", It.IsAny<CancellationToken>()))
            .ReturnsAsync(scenario);

        var result = await GetScenarioAttributionQueryHandler.Handle(
            new GetScenarioAttributionQuery("scenario-1"), _scenarioRepository.Object,
            Mock.Of<ILogger<GetScenarioAttributionQuery>>(), CancellationToken.None);

        result.Should().NotBeNull();
        result!.ContentId.Should().Be("scenario-1");
        result.Credits.Should().HaveCount(1);
    }

    [Fact]
    public async Task GetScenarioAttribution_WithNonExistingScenario_ReturnsNull()
    {
        _scenarioRepository.Setup(r => r.GetByIdAsync("missing", It.IsAny<CancellationToken>()))
            .ReturnsAsync(default(Scenario));

        var result = await GetScenarioAttributionQueryHandler.Handle(
            new GetScenarioAttributionQuery("missing"), _scenarioRepository.Object,
            Mock.Of<ILogger<GetScenarioAttributionQuery>>(), CancellationToken.None);

        result.Should().BeNull();
    }

    #endregion

    #region GetScenarioIpStatusQueryHandler Tests

    [Fact]
    public async Task GetScenarioIpStatus_WithRegisteredScenario_ReturnsStatusWithExplorerUrl()
    {
        var scenario = new Scenario
        {
            Id = "scenario-1",
            Title = "Test Scenario",
            StoryProtocol = new ScenarioStoryProtocol
            {
                IpAssetId = "ip-scenario-456",
                RegisteredAt = DateTime.UtcNow
            }
        };
        _scenarioRepository.Setup(r => r.GetByIdAsync("scenario-1", It.IsAny<CancellationToken>()))
            .ReturnsAsync(scenario);

        var result = await GetScenarioIpStatusQueryHandler.Handle(
            new GetScenarioIpStatusQuery("scenario-1"), _scenarioRepository.Object,
            Mock.Of<ILogger<GetScenarioIpStatusQuery>>(), _options, CancellationToken.None);

        result.Should().NotBeNull();
        result!.IsRegistered.Should().BeTrue();
        result.ExplorerUrl.Should().Be("https://explorer.test/address/ip-scenario-456");
    }

    [Fact]
    public async Task GetScenarioIpStatus_WithUnregisteredScenario_ReturnsNoExplorerUrl()
    {
        var scenario = new Scenario
        {
            Id = "scenario-1",
            Title = "Unregistered",
            StoryProtocol = new ScenarioStoryProtocol()
        };
        _scenarioRepository.Setup(r => r.GetByIdAsync("scenario-1", It.IsAny<CancellationToken>()))
            .ReturnsAsync(scenario);

        var result = await GetScenarioIpStatusQueryHandler.Handle(
            new GetScenarioIpStatusQuery("scenario-1"), _scenarioRepository.Object,
            Mock.Of<ILogger<GetScenarioIpStatusQuery>>(), _options, CancellationToken.None);

        result.Should().NotBeNull();
        result!.IsRegistered.Should().BeFalse();
        result.ExplorerUrl.Should().BeNull();
    }

    #endregion
}
