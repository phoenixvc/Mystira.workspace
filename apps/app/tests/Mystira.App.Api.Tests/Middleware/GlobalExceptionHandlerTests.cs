using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Moq;
using Mystira.App.Domain.Exceptions;
using GlobalExceptionHandler = Mystira.App.Api.Middleware.GlobalExceptionHandler;
using System.Text.Json;

namespace Mystira.App.Api.Tests.Middleware;

public class GlobalExceptionHandlerTests
{
    private readonly Mock<ILogger<GlobalExceptionHandler>> _logger;
    private readonly Mock<IHostEnvironment> _environment;
    private readonly GlobalExceptionHandler _handler;

    public GlobalExceptionHandlerTests()
    {
        _logger = new Mock<ILogger<GlobalExceptionHandler>>();
        _environment = new Mock<IHostEnvironment>();
        _environment.Setup(e => e.EnvironmentName).Returns("Development");
        _handler = new GlobalExceptionHandler(_logger.Object, _environment.Object);
    }

    private static HttpContext CreateHttpContext()
    {
        var context = new DefaultHttpContext();
        context.Request.Path = "/api/test";
        context.Response.Body = new MemoryStream();
        return context;
    }

    private static async Task<ProblemDetails?> GetProblemDetailsFromResponse(HttpContext context)
    {
        context.Response.Body.Seek(0, SeekOrigin.Begin);
        using var reader = new StreamReader(context.Response.Body);
        var json = await reader.ReadToEndAsync();
        return JsonSerializer.Deserialize<ProblemDetails>(json, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });
    }

    #region NotFoundException Tests

    [Fact]
    public async Task TryHandleAsync_WithNotFoundException_Returns404()
    {
        // Arrange
        var context = CreateHttpContext();
        var exception = new NotFoundException("User", "123");

        // Act
        var result = await _handler.TryHandleAsync(context, exception, CancellationToken.None);

        // Assert
        result.Should().BeTrue();
        context.Response.StatusCode.Should().Be(StatusCodes.Status404NotFound);

        var problemDetails = await GetProblemDetailsFromResponse(context);
        problemDetails.Should().NotBeNull();
        problemDetails!.Status.Should().Be(404);
        problemDetails.Title.Should().Be("Resource Not Found");
    }

    [Fact]
    public async Task TryHandleAsync_WithNotFoundException_IncludesErrorCode()
    {
        // Arrange
        var context = CreateHttpContext();
        var exception = new NotFoundException("Account", "acc-456");

        // Act
        await _handler.TryHandleAsync(context, exception, CancellationToken.None);

        // Assert
        var problemDetails = await GetProblemDetailsFromResponse(context);
        problemDetails!.Extensions.Should().ContainKey("errorCode");
    }

    #endregion

    #region ValidationException Tests

    [Fact]
    public async Task TryHandleAsync_WithValidationException_Returns400()
    {
        // Arrange
        var context = CreateHttpContext();
        var exception = new ValidationException("Email", "Invalid email format");

        // Act
        var result = await _handler.TryHandleAsync(context, exception, CancellationToken.None);

        // Assert
        result.Should().BeTrue();
        context.Response.StatusCode.Should().Be(StatusCodes.Status400BadRequest);

        var problemDetails = await GetProblemDetailsFromResponse(context);
        problemDetails.Should().NotBeNull();
        problemDetails!.Status.Should().Be(400);
        problemDetails.Title.Should().Be("Validation Failed");
    }

    [Fact]
    public async Task TryHandleAsync_WithValidationExceptionWithMultipleErrors_IncludesAllErrors()
    {
        // Arrange
        var context = CreateHttpContext();
        var errors = new List<ValidationError>
        {
            new("Email", "Invalid email format"),
            new("Name", "Name is required"),
            new("Age", "Age must be positive")
        };
        var exception = new ValidationException(errors);

        // Act
        await _handler.TryHandleAsync(context, exception, CancellationToken.None);

        // Assert
        var problemDetails = await GetProblemDetailsFromResponse(context);
        problemDetails!.Extensions.Should().ContainKey("errors");
    }

    #endregion

    #region ConflictException Tests

    [Fact]
    public async Task TryHandleAsync_WithConflictException_Returns409()
    {
        // Arrange
        var context = CreateHttpContext();
        var exception = new ConflictException("Resource already exists");

        // Act
        var result = await _handler.TryHandleAsync(context, exception, CancellationToken.None);

        // Assert
        result.Should().BeTrue();
        context.Response.StatusCode.Should().Be(StatusCodes.Status409Conflict);

        var problemDetails = await GetProblemDetailsFromResponse(context);
        problemDetails!.Status.Should().Be(409);
        problemDetails.Title.Should().Be("Resource Conflict");
    }

    #endregion

    #region ForbiddenException Tests

    [Fact]
    public async Task TryHandleAsync_WithForbiddenException_Returns403()
    {
        // Arrange
        var context = CreateHttpContext();
        var exception = new ForbiddenException("Resource", "Access denied to this resource");

        // Act
        var result = await _handler.TryHandleAsync(context, exception, CancellationToken.None);

        // Assert
        result.Should().BeTrue();
        context.Response.StatusCode.Should().Be(StatusCodes.Status403Forbidden);

        var problemDetails = await GetProblemDetailsFromResponse(context);
        problemDetails!.Status.Should().Be(403);
        problemDetails.Title.Should().Be("Access Forbidden");
    }

    #endregion

    #region BusinessRuleException Tests

    [Fact]
    public async Task TryHandleAsync_WithBusinessRuleException_Returns422()
    {
        // Arrange
        var context = CreateHttpContext();
        var exception = new BusinessRuleException("MaxProfiles", "Cannot create more than 5 profiles per account");

        // Act
        var result = await _handler.TryHandleAsync(context, exception, CancellationToken.None);

        // Assert
        result.Should().BeTrue();
        context.Response.StatusCode.Should().Be(StatusCodes.Status422UnprocessableEntity);

        var problemDetails = await GetProblemDetailsFromResponse(context);
        problemDetails!.Status.Should().Be(422);
        problemDetails.Title.Should().Be("Business Rule Violation");
    }

    #endregion

    #region ArgumentException Tests

    [Fact]
    public async Task TryHandleAsync_WithArgumentException_Returns400()
    {
        // Arrange
        var context = CreateHttpContext();
        var exception = new ArgumentException("Parameter cannot be null", "accountId");

        // Act
        var result = await _handler.TryHandleAsync(context, exception, CancellationToken.None);

        // Assert
        result.Should().BeTrue();
        context.Response.StatusCode.Should().Be(StatusCodes.Status400BadRequest);

        var problemDetails = await GetProblemDetailsFromResponse(context);
        problemDetails!.Status.Should().Be(400);
        problemDetails.Title.Should().Be("Invalid Argument");
    }

    #endregion

    #region InvalidOperationException Tests

    [Fact]
    public async Task TryHandleAsync_WithInvalidOperationException_Returns400()
    {
        // Arrange
        var context = CreateHttpContext();
        var exception = new InvalidOperationException("Cannot perform this operation in current state");

        // Act
        var result = await _handler.TryHandleAsync(context, exception, CancellationToken.None);

        // Assert
        result.Should().BeTrue();
        context.Response.StatusCode.Should().Be(StatusCodes.Status400BadRequest);

        var problemDetails = await GetProblemDetailsFromResponse(context);
        problemDetails!.Status.Should().Be(400);
        problemDetails.Title.Should().Be("Invalid Operation");
    }

    #endregion

    #region UnauthorizedAccessException Tests

    [Fact]
    public async Task TryHandleAsync_WithUnauthorizedAccessException_Returns401()
    {
        // Arrange
        var context = CreateHttpContext();
        var exception = new UnauthorizedAccessException();

        // Act
        var result = await _handler.TryHandleAsync(context, exception, CancellationToken.None);

        // Assert
        result.Should().BeTrue();
        context.Response.StatusCode.Should().Be(StatusCodes.Status401Unauthorized);

        var problemDetails = await GetProblemDetailsFromResponse(context);
        problemDetails!.Status.Should().Be(401);
        problemDetails.Title.Should().Be("Unauthorized");
    }

    #endregion

    #region Unhandled Exception Tests

    [Fact]
    public async Task TryHandleAsync_WithUnhandledException_Returns500()
    {
        // Arrange
        var context = CreateHttpContext();
        var exception = new Exception("Something unexpected happened");

        // Act
        var result = await _handler.TryHandleAsync(context, exception, CancellationToken.None);

        // Assert
        result.Should().BeTrue();
        context.Response.StatusCode.Should().Be(StatusCodes.Status500InternalServerError);

        var problemDetails = await GetProblemDetailsFromResponse(context);
        problemDetails!.Status.Should().Be(500);
        problemDetails.Title.Should().Be("Internal Server Error");
    }

    [Fact]
    public async Task TryHandleAsync_WithUnhandledExceptionInDevelopment_IncludesStackTrace()
    {
        // Arrange
        _environment.Setup(e => e.EnvironmentName).Returns("Development");
        var context = CreateHttpContext();
        var exception = new Exception("Test error");

        // Act
        await _handler.TryHandleAsync(context, exception, CancellationToken.None);

        // Assert
        var problemDetails = await GetProblemDetailsFromResponse(context);
        problemDetails!.Extensions.Should().ContainKey("stackTrace");
    }

    [Fact]
    public async Task TryHandleAsync_WithUnhandledExceptionInProduction_ExcludesStackTrace()
    {
        // Arrange
        _environment.Setup(e => e.EnvironmentName).Returns("Production");
        var productionHandler = new GlobalExceptionHandler(_logger.Object, _environment.Object);
        var context = CreateHttpContext();
        var exception = new Exception("Test error");

        // Act
        await productionHandler.TryHandleAsync(context, exception, CancellationToken.None);

        // Assert
        var problemDetails = await GetProblemDetailsFromResponse(context);
        problemDetails!.Extensions.Should().NotContainKey("stackTrace");
    }

    [Fact]
    public async Task TryHandleAsync_WithUnhandledExceptionInProduction_ReturnsGenericMessage()
    {
        // Arrange
        _environment.Setup(e => e.EnvironmentName).Returns("Production");
        var productionHandler = new GlobalExceptionHandler(_logger.Object, _environment.Object);
        var context = CreateHttpContext();
        var exception = new Exception("Sensitive internal error details");

        // Act
        await productionHandler.TryHandleAsync(context, exception, CancellationToken.None);

        // Assert
        var problemDetails = await GetProblemDetailsFromResponse(context);
        problemDetails!.Detail.Should().NotContain("Sensitive internal error details");
        problemDetails.Detail.Should().Be("An unexpected error occurred. Please try again later.");
    }

    #endregion

    #region Common ProblemDetails Fields Tests

    [Fact]
    public async Task TryHandleAsync_AlwaysIncludesTraceId()
    {
        // Arrange
        var context = CreateHttpContext();
        context.TraceIdentifier = "test-trace-id-123";
        var exception = new NotFoundException("Test", "1");

        // Act
        await _handler.TryHandleAsync(context, exception, CancellationToken.None);

        // Assert
        var problemDetails = await GetProblemDetailsFromResponse(context);
        problemDetails!.Extensions.Should().ContainKey("traceId");
    }

    [Fact]
    public async Task TryHandleAsync_AlwaysIncludesTimestamp()
    {
        // Arrange
        var context = CreateHttpContext();
        var exception = new NotFoundException("Test", "1");

        // Act
        await _handler.TryHandleAsync(context, exception, CancellationToken.None);

        // Assert
        var problemDetails = await GetProblemDetailsFromResponse(context);
        problemDetails!.Extensions.Should().ContainKey("timestamp");
    }

    [Fact]
    public async Task TryHandleAsync_IncludesInstancePath()
    {
        // Arrange
        var context = CreateHttpContext();
        context.Request.Path = "/api/users/123";
        var exception = new NotFoundException("User", "123");

        // Act
        await _handler.TryHandleAsync(context, exception, CancellationToken.None);

        // Assert
        var problemDetails = await GetProblemDetailsFromResponse(context);
        problemDetails!.Instance.Should().Be("/api/users/123");
    }

    [Fact]
    public async Task TryHandleAsync_IncludesTypeUri()
    {
        // Arrange
        var context = CreateHttpContext();
        var exception = new NotFoundException("Test", "1");

        // Act
        await _handler.TryHandleAsync(context, exception, CancellationToken.None);

        // Assert
        var problemDetails = await GetProblemDetailsFromResponse(context);
        problemDetails!.Type.Should().Be("https://httpstatuses.com/404");
    }

    #endregion

    #region Logging Tests

    [Fact]
    public async Task TryHandleAsync_WithUnhandledException_LogsError()
    {
        // Arrange
        var context = CreateHttpContext();
        var exception = new Exception("Unhandled error");

        // Act
        await _handler.TryHandleAsync(context, exception, CancellationToken.None);

        // Assert
        _logger.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => true),
                It.IsAny<Exception>(),
                It.Is<Func<It.IsAnyType, Exception?, string>>((v, t) => true)),
            Times.Once);
    }

    [Fact]
    public async Task TryHandleAsync_WithClientError_LogsWarning()
    {
        // Arrange
        var context = CreateHttpContext();
        var exception = new NotFoundException("Test", "1");

        // Act
        await _handler.TryHandleAsync(context, exception, CancellationToken.None);

        // Assert
        _logger.Verify(
            x => x.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => true),
                It.IsAny<Exception>(),
                It.Is<Func<It.IsAnyType, Exception?, string>>((v, t) => true)),
            Times.Once);
    }

    #endregion

    #region Always Returns True Tests

    [Theory]
    [InlineData(typeof(NotFoundException))]
    [InlineData(typeof(ArgumentException))]
    [InlineData(typeof(InvalidOperationException))]
    [InlineData(typeof(UnauthorizedAccessException))]
    [InlineData(typeof(Exception))]
    public async Task TryHandleAsync_AlwaysReturnsTrue(Type exceptionType)
    {
        // Arrange
        var context = CreateHttpContext();
        Exception exception = exceptionType.Name switch
        {
            nameof(NotFoundException) => new NotFoundException("Test", "1"),
            nameof(ArgumentException) => new ArgumentException("test"),
            nameof(InvalidOperationException) => new InvalidOperationException("test"),
            nameof(UnauthorizedAccessException) => new UnauthorizedAccessException(),
            _ => new Exception("test")
        };

        // Act
        var result = await _handler.TryHandleAsync(context, exception, CancellationToken.None);

        // Assert
        result.Should().BeTrue("handler should always return true to indicate the exception was handled");
    }

    #endregion
}
