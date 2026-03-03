using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Moq;
using Mystira.Shared.Middleware;
using Xunit;

namespace Mystira.Shared.Tests.Middleware;

public class CorrelationIdMiddlewareTests
{
    private readonly Mock<RequestDelegate> _nextMock;
    private readonly Mock<ILogger<CorrelationIdMiddleware>> _loggerMock;
    private readonly CorrelationIdMiddleware _middleware;

    public CorrelationIdMiddlewareTests()
    {
        _nextMock = new Mock<RequestDelegate>();
        _loggerMock = new Mock<ILogger<CorrelationIdMiddleware>>();
        _middleware = new CorrelationIdMiddleware(_nextMock.Object, _loggerMock.Object);
    }

    [Fact]
    public async Task InvokeAsync_GeneratesCorrelationId_WhenNotProvided()
    {
        // Arrange
        var context = new DefaultHttpContext();

        // Act
        await _middleware.InvokeAsync(context);

        // Assert
        context.Items.Should().ContainKey("CorrelationId");
        context.Items["CorrelationId"]
            .Should()
            .BeOfType<string>()
            .Which
            .Should()
            .NotBeNullOrWhiteSpace()
            .And
            .HaveLength(32); // GUID without hyphens
    }

    [Fact]
    public async Task InvokeAsync_UsesProvidedCorrelationId_FromPrimaryHeader()
    {
        // Arrange
        var context = new DefaultHttpContext();
        var expectedId = "test-correlation-id-123";
        context.Request.Headers["X-Correlation-Id"] = expectedId;

        // Act
        await _middleware.InvokeAsync(context);

        // Assert
        context.Items["CorrelationId"].Should().Be(expectedId);
    }

    [Theory]
    [InlineData("X-Request-Id")]
    [InlineData("Request-Id")]
    public async Task InvokeAsync_UsesAlternativeHeaders(string headerName)
    {
        // Arrange
        var context = new DefaultHttpContext();
        var expectedId = "alternative-correlation-id";
        context.Request.Headers[headerName] = expectedId;

        // Act
        await _middleware.InvokeAsync(context);

        // Assert
        context.Items["CorrelationId"].Should().Be(expectedId);
    }

    [Fact]
    public async Task InvokeAsync_ExtractsTraceIdFromTraceparent()
    {
        // Arrange
        var context = new DefaultHttpContext();
        var traceId = "4bf92f3577b34da6a3ce929d0e0e4736";
        var traceparent = $"00-{traceId}-00f067aa0ba902b7-01";
        context.Request.Headers["traceparent"] = traceparent;

        // Act
        await _middleware.InvokeAsync(context);

        // Assert
        context.Items["CorrelationId"].Should().Be(traceId);
    }

    [Fact]
    public async Task InvokeAsync_AddsCorrelationIdToResponse()
    {
        // Arrange
        var context = new DefaultHttpContext();
        var expectedId = "response-correlation-id";
        context.Request.Headers["X-Correlation-Id"] = expectedId;

        // Act
        await _middleware.InvokeAsync(context);
        
        // Trigger OnStarting callbacks
        await context.Response.StartAsync();

        // Assert
        context.Response.Headers.Should().ContainKey("X-Correlation-Id");
        context.Response.Headers["X-Correlation-Id"].ToString().Should().Be(expectedId);
    }

    [Fact]
    public async Task InvokeAsync_CallsNextMiddleware()
    {
        // Arrange
        var context = new DefaultHttpContext();
        var nextCalled = false;
        _nextMock.Setup(next => next(It.IsAny<HttpContext>()))
            .Callback(() => nextCalled = true)
            .Returns(Task.CompletedTask);

        // Act
        await _middleware.InvokeAsync(context);

        // Assert
        nextCalled.Should().BeTrue();
        _nextMock.Verify(next => next(context), Times.Once);
    }

    [Fact]
    public async Task InvokeAsync_PrefersPrimaryHeader_OverAlternatives()
    {
        // Arrange
        var context = new DefaultHttpContext();
        var primaryId = "primary-id";
        var alternativeId = "alternative-id";
        context.Request.Headers["X-Correlation-Id"] = primaryId;
        context.Request.Headers["X-Request-Id"] = alternativeId;

        // Act
        await _middleware.InvokeAsync(context);

        // Assert
        context.Items["CorrelationId"].Should().Be(primaryId);
    }

    [Fact]
    public void Constructor_ThrowsArgumentNullException_WhenNextIsNull()
    {
        // Act
        Action act = () => new CorrelationIdMiddleware(null!, _loggerMock.Object);

        // Assert
        act.Should().Throw<ArgumentNullException>().WithParameterName("next");
    }

    [Fact]
    public void Constructor_ThrowsArgumentNullException_WhenLoggerIsNull()
    {
        // Act
        Action act = () => new CorrelationIdMiddleware(_nextMock.Object, null!);

        // Assert
        act.Should().Throw<ArgumentNullException>().WithParameterName("logger");
    }

    [Fact]
    public async Task InvokeAsync_GeneratesUniqueIds_ForMultipleRequests()
    {
        // Arrange
        var context1 = new DefaultHttpContext();
        var context2 = new DefaultHttpContext();

        // Act
        await _middleware.InvokeAsync(context1);
        await _middleware.InvokeAsync(context2);

        // Assert
        var id1 = context1.Items["CorrelationId"] as string;
        var id2 = context2.Items["CorrelationId"] as string;
        id1.Should().NotBeNullOrWhiteSpace();
        id2.Should().NotBeNullOrWhiteSpace();
        id1.Should().NotBe(id2);
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("   ")]
    public async Task InvokeAsync_GeneratesNewId_WhenProvidedIdIsWhitespace(string whitespaceId)
    {
        // Arrange
        var context = new DefaultHttpContext();
        context.Request.Headers["X-Correlation-Id"] = whitespaceId;

        // Act
        await _middleware.InvokeAsync(context);

        // Assert
        var correlationId = context.Items["CorrelationId"] as string;
        correlationId.Should().NotBeNull();
        correlationId.Should().NotBeNullOrWhiteSpace();
        correlationId.Should().NotBe(whitespaceId.Trim());
    }
}
