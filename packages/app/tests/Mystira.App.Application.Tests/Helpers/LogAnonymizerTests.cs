using FluentAssertions;
using Mystira.App.Application.Helpers;
using Xunit;

namespace Mystira.App.Application.Tests.Helpers;

public class LogAnonymizerTests
{
    [Fact]
    public void HashId_ReturnsConsistentHash_ForSameInput()
    {
        var hash1 = LogAnonymizer.HashId("account-123");
        var hash2 = LogAnonymizer.HashId("account-123");

        hash1.Should().Be(hash2);
    }

    [Fact]
    public void HashId_ReturnsDifferentHashes_ForDifferentInputs()
    {
        var hash1 = LogAnonymizer.HashId("account-123");
        var hash2 = LogAnonymizer.HashId("account-456");

        hash1.Should().NotBe(hash2);
    }

    [Fact]
    public void HashId_Returns8CharHexString()
    {
        var hash = LogAnonymizer.HashId("some-id");

        hash.Should().HaveLength(8);
        hash.Should().MatchRegex("^[0-9a-f]{8}$");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    public void HashId_ReturnsPlaceholder_ForNullOrEmpty(string? id)
    {
        LogAnonymizer.HashId(id).Should().Be("[empty]");
    }

    [Fact]
    public void HashEmail_NormalizesCase()
    {
        var hash1 = LogAnonymizer.HashEmail("User@Example.COM");
        var hash2 = LogAnonymizer.HashEmail("user@example.com");

        hash1.Should().Be(hash2);
    }

    [Fact]
    public void HashEmail_TrimsWhitespace()
    {
        var hash1 = LogAnonymizer.HashEmail("  user@example.com  ");
        var hash2 = LogAnonymizer.HashEmail("user@example.com");

        hash1.Should().Be(hash2);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    public void HashEmail_ReturnsPlaceholder_ForNullOrEmpty(string? email)
    {
        LogAnonymizer.HashEmail(email).Should().Be("[empty]");
    }

    [Fact]
    public void HashId_DoesNotContainOriginalInput()
    {
        var input = "account-12345678";
        var hash = LogAnonymizer.HashId(input);

        hash.Should().NotContain("account");
        hash.Should().NotContain("12345678");
    }
}
