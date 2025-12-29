using FluentAssertions;
using Mystira.Shared.Logging;
using Xunit;

namespace Mystira.Shared.Tests.Logging;

public class PiiRedactorTests
{
    [Theory]
    [InlineData("user@example.com", "***@***.***")]
    [InlineData("test.user+tag@domain.co.uk", "***@***.***")]
    [InlineData("simple@test.com", "***@***.***")]
    public void RedactEmail_RedactsEmailAddresses(string email, string expected)
    {
        // Act
        var result = PiiRedactor.RedactEmail(email);

        // Assert
        result.Should().Be(expected);
    }

    [Fact]
    public void RedactEmailsInString_RedactsEmailsInText()
    {
        // Arrange
        var text = "Contact us at support@example.com or admin@test.org";

        // Act
        var result = PiiRedactor.RedactEmailsInString(text);

        // Assert
        result.Should().Contain("***@");
        result.Should().NotContain("support@example.com");
        result.Should().NotContain("admin@test.org");
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(null)]
    public void RedactEmailsInString_HandlesEmptyOrNullInput(string? input)
    {
        // Act
        var result = PiiRedactor.RedactEmailsInString(input!);

        // Assert
        result.Should().Be(input ?? string.Empty);
    }

    [Fact]
    public void RedactEmailsInString_DoesNotModifyTextWithoutPii()
    {
        // Arrange
        var text = "This is a normal text without any PII data.";

        // Act
        var result = PiiRedactor.RedactEmailsInString(text);

        // Assert
        result.Should().Be(text);
    }

    [Fact]
    public void RedactDisplayName_RedactsName()
    {
        // Arrange
        var name = "John Doe";

        // Act
        var result = PiiRedactor.RedactDisplayName(name);

        // Assert
        result.Should().Be("J***");
    }

    [Fact]
    public void HashEmail_ReturnsConsistentHash()
    {
        // Arrange
        var email = "test@example.com";

        // Act
        var hash1 = PiiRedactor.HashEmail(email);
        var hash2 = PiiRedactor.HashEmail(email);

        // Assert
        hash1.Should().Be(hash2);
        hash1.Should().StartWith("user-");
    }

    [Fact]
    public void MaskIp_MasksIPv4Address()
    {
        // Arrange
        var ip = "192.168.1.100";

        // Act
        var result = PiiRedactor.MaskIp(ip);

        // Assert
        result.Should().Be("192.168.xxx.xxx");
    }

    [Fact]
    public void MaskIp_MasksIPv6Address()
    {
        // Arrange
        var ip = "2001:0db8:85a3:0000:0000:8a2e:0370:7334";

        // Act
        var result = PiiRedactor.MaskIp(ip);

        // Assert
        result.Should().Contain("xxxx");
    }

    [Fact]
    public void SanitizeLogInput_RemovesNewlines()
    {
        // Arrange
        var input = "Line 1\r\nLine 2\nLine 3";

        // Act
        var result = PiiRedactor.SanitizeLogInput(input);

        // Assert
        result.Should().NotContain("\r");
        result.Should().NotContain("\n");
    }

    [Fact]
    public void SanitizeLogInput_TruncatesLongInput()
    {
        // Arrange
        var input = new string('x', 300);

        // Act
        var result = PiiRedactor.SanitizeLogInput(input, 200);

        // Assert
        result.Should().Contain("...[truncated]");
        result.Length.Should().BeLessThan(250);
    }

    [Fact]
    public void CreateSafeLogEntry_CreatesHashedEntry()
    {
        // Arrange
        var email = "user@example.com";
        var action = "Login";

        // Act
        var result = PiiRedactor.CreateSafeLogEntry(email, action);

        // Assert
        result.Should().Contain("Login");
        result.Should().StartWith("[user-");
        result.Should().NotContain("user@example.com");
    }
}
