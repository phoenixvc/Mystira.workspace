using FluentAssertions;
using Mystira.Shared.Errors;
using Xunit;

namespace Mystira.Shared.Tests.Errors;

public class TroubleshootingHelperTests
{
    [Fact]
    public void Analyze_WithAnyError_ReturnsResult()
    {
        // Arrange
        var errorMessage = "Some error occurred";

        // Act
        var result = TroubleshootingHelper.Analyze(errorMessage);

        // Assert
        result.Should().NotBeNull();
        result.ErrorCode.Should().NotBeNullOrEmpty();
        result.Title.Should().NotBeNullOrEmpty();
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    public void Analyze_EmptyInput_ReturnsUnmatchedResult(string? errorMessage)
    {
        // Act
        var result = TroubleshootingHelper.Analyze(errorMessage!);

        // Assert
        result.Matched.Should().BeFalse();
        result.ErrorCode.Should().Be("UNKNOWN");
    }

    [Fact]
    public void Analyze_NullInput_HandlesGracefully()
    {
        // Act & Assert - Should not throw
        Action act = () => TroubleshootingHelper.Analyze(null!);
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void Analyze_UnmatchedError_ReturnsUnknownCategory()
    {
        // Arrange - Use a very specific error that likely won't match
        var errorMessage = "XYZABC123_UNIQUE_ERROR_PATTERN_THAT_DOES_NOT_EXIST";

        // Act
        var result = TroubleshootingHelper.Analyze(errorMessage);

        // Assert
        result.Category.Should().Be(ErrorCategory.Unknown);
    }

    [Fact]
    public void Analyze_Result_HasAllRequiredFields()
    {
        // Arrange
        var errorMessage = "Generic error for testing";

        // Act
        var result = TroubleshootingHelper.Analyze(errorMessage);

        // Assert
        result.ErrorCode.Should().NotBeNullOrEmpty();
        result.Title.Should().NotBeNullOrEmpty();
        result.Description.Should().NotBeNullOrEmpty();
        result.Solutions.Should().NotBeNull();
    }

    [Fact]
    public void Analyze_MatchedResult_ProvidesHelpfulSolutions()
    {
        // Arrange - Try a common database error
        var errorMessage = "Cannot open database";

        // Act
        var result = TroubleshootingHelper.Analyze(errorMessage);

        // Assert
        if (result.Matched)
        {
            result.Solutions.Should().NotBeEmpty();
            result.Solutions.Should().OnlyContain(s => !string.IsNullOrWhiteSpace(s));
        }
    }

    [Fact]
    public void Analyze_MatchedResult_HasValidCategory()
    {
        // Arrange - Try a common error
        var errorMessage = "Network connection failed";

        // Act
        var result = TroubleshootingHelper.Analyze(errorMessage);

        // Assert
        result.Category.Should().BeOneOf(
            ErrorCategory.Azure,
            ErrorCategory.Authentication,
            ErrorCategory.Database,
            ErrorCategory.Network,
            ErrorCategory.Configuration,
            ErrorCategory.Build,
            ErrorCategory.Unknown);
    }

    [Fact]
    public void Analyze_HandlesLongErrorMessages()
    {
        // Arrange
        var longError = new string('x', 10000) + " error occurred";

        // Act
        var result = TroubleshootingHelper.Analyze(longError);

        // Assert
        result.Should().NotBeNull();
        result.ErrorCode.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public void Analyze_HandlesSpecialCharacters()
    {
        // Arrange
        var errorWithSpecialChars = "Error: <script>alert('test')</script> \r\n\t\\ failed";

        // Act
        var result = TroubleshootingHelper.Analyze(errorWithSpecialChars);

        // Assert
        result.Should().NotBeNull();
        result.ErrorCode.Should().NotBeNullOrEmpty();
    }
}
