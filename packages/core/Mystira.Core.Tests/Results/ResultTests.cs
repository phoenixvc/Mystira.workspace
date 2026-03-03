using FluentAssertions;
using Mystira.Core.Results;

namespace Mystira.Core.Tests.Results;

public class ResultTests
{
    [Fact]
    public void Success_ShouldCreateSuccessfulResult()
    {
        // Arrange & Act
        var result = Result<int>.Success(42);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.IsFailure.Should().BeFalse();
        result.Value.Should().Be(42);
    }

    [Fact]
    public void Failure_ShouldCreateFailedResult()
    {
        // Arrange
        var error = Error.NotFound("User", "123");

        // Act
        var result = Result<int>.Failure(error);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(error);
    }

    [Fact]
    public void Value_OnFailure_ShouldThrow()
    {
        // Arrange
        var result = Result<int>.Failure(Error.NotFound("User"));

        // Act
        var act = () => result.Value;

        // Assert
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*failed Result*");
    }

    [Fact]
    public void Error_OnSuccess_ShouldThrow()
    {
        // Arrange
        var result = Result<int>.Success(42);

        // Act
        var act = () => result.Error;

        // Assert
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*successful Result*");
    }

    [Fact]
    public void Match_OnSuccess_ShouldCallOnSuccess()
    {
        // Arrange
        var result = Result<int>.Success(42);

        // Act
        var message = result.Match(
            onSuccess: v => $"Value: {v}",
            onFailure: e => $"Error: {e.Message}");

        // Assert
        message.Should().Be("Value: 42");
    }

    [Fact]
    public void Match_OnFailure_ShouldCallOnFailure()
    {
        // Arrange
        var result = Result<int>.Failure(Error.NotFound("User"));

        // Act
        var message = result.Match(
            onSuccess: v => $"Value: {v}",
            onFailure: e => $"Error: {e.Message}");

        // Assert
        message.Should().StartWith("Error:");
    }

    [Fact]
    public void ImplicitConversion_FromValue_ShouldCreateSuccess()
    {
        // Arrange & Act
        Result<int> result = 42;

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(42);
    }

    [Fact]
    public void ImplicitConversion_FromError_ShouldCreateFailure()
    {
        // Arrange & Act
        Result<int> result = Error.NotFound("User");

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("NOT_FOUND");
    }

    [Fact]
    public void GetValueOrDefault_OnSuccess_ShouldReturnValue()
    {
        // Arrange
        var result = Result<int>.Success(42);

        // Act
        var value = result.GetValueOrDefault(0);

        // Assert
        value.Should().Be(42);
    }

    [Fact]
    public void GetValueOrDefault_OnFailure_ShouldReturnDefault()
    {
        // Arrange
        var result = Result<int>.Failure(Error.NotFound("User"));

        // Act
        var value = result.GetValueOrDefault(0);

        // Assert
        value.Should().Be(0);
    }
}
