using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using Mystira.App.Api.Controllers;
using Mystira.App.Application.CQRS.Attribution.Queries;
using Mystira.App.Application.CQRS.ContentBundles.Queries;
using Mystira.App.Domain.Models;
using Mystira.Contracts.App.Responses.Attribution;
using Wolverine;

namespace Mystira.App.Api.Tests.Controllers;

public class BundlesControllerTests
{
    private readonly Mock<IMessageBus> _mockBus;
    private readonly Mock<ILogger<BundlesController>> _mockLogger;
    private readonly BundlesController _controller;

    public BundlesControllerTests()
    {
        _mockBus = new Mock<IMessageBus>();
        _mockLogger = new Mock<ILogger<BundlesController>>();
        _controller = new BundlesController(_mockBus.Object, _mockLogger.Object);

        var httpContext = new DefaultHttpContext();
        httpContext.TraceIdentifier = "test-trace-id";
        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = httpContext
        };
    }

    #region GetBundles Tests

    [Fact]
    public async Task GetBundles_ReturnsOkWithBundleList()
    {
        var bundles = new List<ContentBundle>
        {
            new() { Id = "bundle-1", Title = "Adventure Pack" },
            new() { Id = "bundle-2", Title = "Mystery Pack" }
        };

        _mockBus.Setup(x => x.InvokeAsync<IEnumerable<ContentBundle>>(
                It.IsAny<GetAllContentBundlesQuery>(),
                It.IsAny<CancellationToken>(),
                It.IsAny<TimeSpan?>()))
            .ReturnsAsync(bundles);

        var result = await _controller.GetBundles();

        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var returnedBundles = okResult.Value.Should().BeAssignableTo<IEnumerable<ContentBundle>>().Subject;
        returnedBundles.Should().HaveCount(2);
    }



    #endregion

    #region GetBundlesByAgeGroup Tests

    [Fact]
    public async Task GetBundlesByAgeGroup_ReturnsOkWithBundles()
    {
        var bundles = new List<ContentBundle>
        {
            new() { Id = "bundle-1", Title = "Kids Pack" }
        };

        _mockBus.Setup(x => x.InvokeAsync<IEnumerable<ContentBundle>>(
                It.IsAny<GetContentBundlesByAgeGroupQuery>(),
                It.IsAny<CancellationToken>(),
                It.IsAny<TimeSpan?>()))
            .ReturnsAsync(bundles);

        var result = await _controller.GetBundlesByAgeGroup("Ages7to9");

        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var returnedBundles = okResult.Value.Should().BeAssignableTo<IEnumerable<ContentBundle>>().Subject;
        returnedBundles.Should().HaveCount(1);
    }

    [Fact]
    public async Task GetBundlesByAgeGroup_WithInvalidAgeGroup_ReturnsBadRequest()
    {
        _mockBus.Setup(x => x.InvokeAsync<IEnumerable<ContentBundle>>(
                It.IsAny<GetContentBundlesByAgeGroupQuery>(),
                It.IsAny<CancellationToken>(),
                It.IsAny<TimeSpan?>()))
            .ThrowsAsync(new ArgumentException("Invalid age group"));

        var result = await _controller.GetBundlesByAgeGroup("invalid");

        result.Result.Should().BeOfType<BadRequestObjectResult>();
    }



    #endregion

    #region GetBundleAttribution Tests

    [Fact]
    public async Task GetBundleAttribution_WhenBundleExists_ReturnsOk()
    {
        var attribution = new ContentAttributionResponse
        {
            ContentId = "bundle-1",
            ContentTitle = "Adventure Pack"
        };

        _mockBus.Setup(x => x.InvokeAsync<ContentAttributionResponse?>(
                It.IsAny<GetBundleAttributionQuery>(),
                It.IsAny<CancellationToken>(),
                It.IsAny<TimeSpan?>()))
            .ReturnsAsync(attribution);

        var result = await _controller.GetBundleAttribution("bundle-1");

        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var returned = okResult.Value.Should().BeOfType<ContentAttributionResponse>().Subject;
        returned.ContentId.Should().Be("bundle-1");
    }

    [Fact]
    public async Task GetBundleAttribution_WhenBundleNotFound_ReturnsNotFound()
    {
        _mockBus.Setup(x => x.InvokeAsync<ContentAttributionResponse?>(
                It.IsAny<GetBundleAttributionQuery>(),
                It.IsAny<CancellationToken>(),
                It.IsAny<TimeSpan?>()))
            .ReturnsAsync(default(ContentAttributionResponse));

        var result = await _controller.GetBundleAttribution("nonexistent");

        result.Result.Should().BeOfType<NotFoundObjectResult>();
    }

    #endregion

    #region GetBundleIpStatus Tests

    [Fact]
    public async Task GetBundleIpStatus_WhenBundleExists_ReturnsOk()
    {
        var ipStatus = new IpVerificationResponse
        {
            ContentId = "bundle-1",
            IsRegistered = true
        };

        _mockBus.Setup(x => x.InvokeAsync<IpVerificationResponse?>(
                It.IsAny<GetBundleIpStatusQuery>(),
                It.IsAny<CancellationToken>(),
                It.IsAny<TimeSpan?>()))
            .ReturnsAsync(ipStatus);

        var result = await _controller.GetBundleIpStatus("bundle-1");

        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var returned = okResult.Value.Should().BeOfType<IpVerificationResponse>().Subject;
        returned.IsRegistered.Should().BeTrue();
    }

    [Fact]
    public async Task GetBundleIpStatus_WhenBundleNotFound_ReturnsNotFound()
    {
        _mockBus.Setup(x => x.InvokeAsync<IpVerificationResponse?>(
                It.IsAny<GetBundleIpStatusQuery>(),
                It.IsAny<CancellationToken>(),
                It.IsAny<TimeSpan?>()))
            .ReturnsAsync(default(IpVerificationResponse));

        var result = await _controller.GetBundleIpStatus("nonexistent");

        result.Result.Should().BeOfType<NotFoundObjectResult>();
    }

    #endregion
}
