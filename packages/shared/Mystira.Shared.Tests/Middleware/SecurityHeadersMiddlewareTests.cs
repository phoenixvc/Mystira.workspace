using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using Moq;
using Mystira.Shared.Middleware;
using Xunit;

namespace Mystira.Shared.Tests.Middleware;

public class SecurityHeadersMiddlewareTests
{
    private readonly Mock<RequestDelegate> _nextMock;

    public SecurityHeadersMiddlewareTests()
    {
        _nextMock = new Mock<RequestDelegate>();
        _nextMock.Setup(next => next(It.IsAny<HttpContext>())).Returns(Task.CompletedTask);
    }

    [Fact]
    public async Task InvokeAsync_AddsXContentTypeOptionsHeader()
    {
        // Arrange
        var context = new DefaultHttpContext();
        var middleware = new SecurityHeadersMiddleware(_nextMock.Object);

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        context.Response.Headers.Should().ContainKey("X-Content-Type-Options");
        context.Response.Headers["X-Content-Type-Options"].ToString().Should().Be("nosniff");
    }

    [Fact]
    public async Task InvokeAsync_AddsXFrameOptionsHeader()
    {
        // Arrange
        var context = new DefaultHttpContext();
        var middleware = new SecurityHeadersMiddleware(_nextMock.Object);

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        context.Response.Headers.Should().ContainKey("X-Frame-Options");
        context.Response.Headers["X-Frame-Options"].ToString().Should().Be("DENY");
    }

    [Fact]
    public async Task InvokeAsync_AddsXXSSProtectionHeader()
    {
        // Arrange
        var context = new DefaultHttpContext();
        var middleware = new SecurityHeadersMiddleware(_nextMock.Object);

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        context.Response.Headers.Should().ContainKey("X-XSS-Protection");
        context.Response.Headers["X-XSS-Protection"].ToString().Should().Be("1; mode=block");
    }

    [Fact]
    public async Task InvokeAsync_AddsReferrerPolicyHeader()
    {
        // Arrange
        var context = new DefaultHttpContext();
        var middleware = new SecurityHeadersMiddleware(_nextMock.Object);

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        context.Response.Headers.Should().ContainKey("Referrer-Policy");
        context.Response.Headers["Referrer-Policy"].ToString().Should().Be("strict-origin-when-cross-origin");
    }

    [Fact]
    public async Task InvokeAsync_AddsContentSecurityPolicyHeader()
    {
        // Arrange
        var context = new DefaultHttpContext();
        var middleware = new SecurityHeadersMiddleware(_nextMock.Object);

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        context.Response.Headers.Should().ContainKey("Content-Security-Policy");
        var csp = context.Response.Headers["Content-Security-Policy"].ToString();
        csp.Should().NotBeNullOrWhiteSpace();
    }

    [Fact]
    public async Task InvokeAsync_AddsPermissionsPolicyHeader()
    {
        // Arrange
        var context = new DefaultHttpContext();
        var middleware = new SecurityHeadersMiddleware(_nextMock.Object);

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        context.Response.Headers.Should().ContainKey("Permissions-Policy");
        context.Response.Headers["Permissions-Policy"].ToString().Should().Contain("geolocation=()");
    }

    [Fact]
    public async Task InvokeAsync_WithStrictCsp_DoesNotAllowUnsafeInline()
    {
        // Arrange
        var context = new DefaultHttpContext();
        var options = Options.Create(new SecurityHeadersOptions { UseStrictCsp = true });
        var middleware = new SecurityHeadersMiddleware(_nextMock.Object, options);

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        var csp = context.Response.Headers["Content-Security-Policy"].ToString();
        csp.Should().NotContain("unsafe-inline");
        csp.Should().NotContain("unsafe-eval");
    }

    [Fact]
    public async Task InvokeAsync_WithNonStrictCsp_AllowsUnsafeInline()
    {
        // Arrange
        var context = new DefaultHttpContext();
        var options = Options.Create(new SecurityHeadersOptions { UseStrictCsp = false });
        var middleware = new SecurityHeadersMiddleware(_nextMock.Object, options);

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        var csp = context.Response.Headers["Content-Security-Policy"].ToString();
        csp.Should().Contain("unsafe-inline");
    }

    [Fact]
    public async Task InvokeAsync_WithNonce_GeneratesAndStoresNonce()
    {
        // Arrange
        var context = new DefaultHttpContext();
        var options = Options.Create(new SecurityHeadersOptions { UseNonce = true });
        var middleware = new SecurityHeadersMiddleware(_nextMock.Object, options);

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        context.Items.Should().ContainKey("CspNonce");
        context.Items["CspNonce"].Should().BeOfType<string>()
            .Which.Should().NotBeNullOrWhiteSpace();
        
        var nonce = (string)context.Items["CspNonce"]!;
        var csp = context.Response.Headers["Content-Security-Policy"].ToString();
        csp.Should().Contain($"nonce-{nonce}");
    }

    [Fact]
    public async Task InvokeAsync_WithAdditionalScriptSources_IncludesThemInCsp()
    {
        // Arrange
        var context = new DefaultHttpContext();
        var options = Options.Create(new SecurityHeadersOptions 
        { 
            AdditionalScriptSources = new[] { "https://cdn.example.com" }
        });
        var middleware = new SecurityHeadersMiddleware(_nextMock.Object, options);

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        var csp = context.Response.Headers["Content-Security-Policy"].ToString();
        csp.Should().Contain("https://cdn.example.com");
    }

    [Fact]
    public async Task InvokeAsync_WithAdditionalStyleSources_IncludesThemInCsp()
    {
        // Arrange
        var context = new DefaultHttpContext();
        var options = Options.Create(new SecurityHeadersOptions 
        { 
            AdditionalStyleSources = new[] { "https://fonts.googleapis.com" }
        });
        var middleware = new SecurityHeadersMiddleware(_nextMock.Object, options);

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        var csp = context.Response.Headers["Content-Security-Policy"].ToString();
        csp.Should().Contain("https://fonts.googleapis.com");
    }

    [Fact]
    public async Task InvokeAsync_WithCustomFrameAncestors_UsesCustomValue()
    {
        // Arrange
        var context = new DefaultHttpContext();
        var options = Options.Create(new SecurityHeadersOptions 
        { 
            FrameAncestors = "'self'" 
        });
        var middleware = new SecurityHeadersMiddleware(_nextMock.Object, options);

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        context.Response.Headers["X-Frame-Options"].ToString().Should().Be("SAMEORIGIN");
        var csp = context.Response.Headers["Content-Security-Policy"].ToString();
        csp.Should().Contain("frame-ancestors 'self'");
    }

    [Fact]
    public async Task InvokeAsync_CallsNextMiddleware()
    {
        // Arrange
        var context = new DefaultHttpContext();
        var middleware = new SecurityHeadersMiddleware(_nextMock.Object);

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        _nextMock.Verify(next => next(context), Times.Once);
    }

    [Fact]
    public async Task InvokeAsync_WithoutOptions_UsesDefaults()
    {
        // Arrange
        var context = new DefaultHttpContext();
        var middleware = new SecurityHeadersMiddleware(_nextMock.Object, null);

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        context.Response.Headers.Should().ContainKey("Content-Security-Policy");
        context.Response.Headers.Should().ContainKey("X-Frame-Options");
    }

    [Fact]
    public async Task InvokeAsync_GeneratesUniqueNonces_ForMultipleRequests()
    {
        // Arrange
        var context1 = new DefaultHttpContext();
        var context2 = new DefaultHttpContext();
        var options = Options.Create(new SecurityHeadersOptions { UseNonce = true });
        var middleware = new SecurityHeadersMiddleware(_nextMock.Object, options);

        // Act
        await middleware.InvokeAsync(context1);
        await middleware.InvokeAsync(context2);

        // Assert
        var nonce1 = context1.Items["CspNonce"] as string;
        var nonce2 = context2.Items["CspNonce"] as string;
        nonce1.Should().NotBeNull();
        nonce2.Should().NotBeNull();
        nonce1.Should().NotBe(nonce2);
    }
}
