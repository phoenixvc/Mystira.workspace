using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Mystira.Core.Services;

namespace Mystira.App.Application.Tests.Services;

public class ConsentEmailBuilderTests
{
    #region Constructor / Configuration

    [Fact]
    public void Constructor_ReadsBaseUrl_FromCoppaVerificationBaseUrl()
    {
        // Arrange
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Coppa:VerificationBaseUrl"] = "https://coppa.mystira.app",
                ["AppSettings:BaseUrl"] = "https://app.mystira.app"
            })
            .Build();

        var builder = new ConsentEmailBuilder(config);

        // Act
        var html = builder.BuildVerificationEmail("TestChild", "token123");

        // Assert - should use Coppa:VerificationBaseUrl (higher priority)
        html.Should().Contain("https://coppa.mystira.app/api/coppa/consent/verify");
        html.Should().NotContain("https://app.mystira.app/api/coppa/consent/verify");
    }

    [Fact]
    public void Constructor_FallsBackToAppSettingsBaseUrl_WhenCoppaVerificationBaseUrlMissing()
    {
        // Arrange
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["AppSettings:BaseUrl"] = "https://test.mystira.app"
            })
            .Build();

        var builder = new ConsentEmailBuilder(config);

        // Act
        var html = builder.BuildVerificationEmail("TestChild", "token123");

        // Assert
        html.Should().Contain("https://test.mystira.app/api/coppa/consent/verify");
    }

    [Fact]
    public void Constructor_FallsBackToDefaultBaseUrl_WhenNoConfigProvided()
    {
        // Arrange
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>())
            .Build();

        var builder = new ConsentEmailBuilder(config);

        // Act
        var html = builder.BuildVerificationEmail("TestChild", "token123");

        // Assert
        html.Should().Contain("https://mystira.app/api/coppa/consent/verify");
    }

    [Fact]
    public void Constructor_ReadsPrivacyPolicyUrl_FromConfig()
    {
        // Arrange
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["AppSettings:BaseUrl"] = "https://test.mystira.app",
                ["Coppa:PrivacyPolicyUrl"] = "https://legal.mystira.app/privacy"
            })
            .Build();

        var builder = new ConsentEmailBuilder(config);

        // Act
        var html = builder.BuildVerificationEmail("TestChild", "token123");

        // Assert
        html.Should().Contain("https://legal.mystira.app/privacy");
    }

    [Fact]
    public void Constructor_FallsBackToBaseUrlPlusPrivacy_WhenPrivacyPolicyUrlMissing()
    {
        // Arrange
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["AppSettings:BaseUrl"] = "https://test.mystira.app"
            })
            .Build();

        var builder = new ConsentEmailBuilder(config);

        // Act
        var html = builder.BuildVerificationEmail("TestChild", "token123");

        // Assert
        html.Should().Contain("https://test.mystira.app/privacy");
    }

    #endregion

    #region Subject Property

    [Fact]
    public void Subject_ReturnsExpectedString()
    {
        // Arrange
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["AppSettings:BaseUrl"] = "https://test.mystira.app"
            })
            .Build();

        var builder = new ConsentEmailBuilder(config);

        // Act & Assert
        builder.Subject.Should().Be("Mystira - Parental Consent Required");
    }

    #endregion

    #region BuildVerificationEmail - Child Display Name

    [Fact]
    public void BuildVerificationEmail_ContainsChildDisplayName()
    {
        // Arrange
        var builder = CreateBuilder();

        // Act
        var html = builder.BuildVerificationEmail("StarExplorer", "token123");

        // Assert
        html.Should().Contain("StarExplorer");
    }

    [Fact]
    public void BuildVerificationEmail_WrapsChildNameInStrongTag()
    {
        // Arrange
        var builder = CreateBuilder();

        // Act
        var html = builder.BuildVerificationEmail("StarExplorer", "token123");

        // Assert
        html.Should().Contain("<strong>StarExplorer</strong>");
    }

    #endregion

    #region BuildVerificationEmail - Verification URL

    [Fact]
    public void BuildVerificationEmail_ContainsVerificationUrlWithToken()
    {
        // Arrange
        var builder = CreateBuilder();

        // Act
        var html = builder.BuildVerificationEmail("TestChild", "abc123");

        // Assert
        html.Should().Contain("https://test.mystira.app/api/coppa/consent/verify?token=abc123");
    }

    [Fact]
    public void BuildVerificationEmail_UrlEncodesToken()
    {
        // Arrange
        var builder = CreateBuilder();

        // Act
        var html = builder.BuildVerificationEmail("TestChild", "token with spaces&special=chars");

        // Assert - token should be URL-encoded
        html.Should().Contain("token%20with%20spaces%26special%3Dchars");
        html.Should().NotContain("token with spaces&special=chars");
    }

    [Fact]
    public void BuildVerificationEmail_VerificationUrlIsInAnchorTag()
    {
        // Arrange
        var builder = CreateBuilder();

        // Act
        var html = builder.BuildVerificationEmail("TestChild", "token123");

        // Assert
        html.Should().Contain("href=\"https://test.mystira.app/api/coppa/consent/verify?token=token123\"");
    }

    #endregion

    #region BuildVerificationEmail - Privacy Policy Link

    [Fact]
    public void BuildVerificationEmail_ContainsPrivacyPolicyLink()
    {
        // Arrange
        var builder = CreateBuilder();

        // Act
        var html = builder.BuildVerificationEmail("TestChild", "token123");

        // Assert
        html.Should().Contain("https://test.mystira.app/privacy");
        html.Should().Contain("Privacy Policy");
    }

    [Fact]
    public void BuildVerificationEmail_PrivacyPolicyIsAnchorTag()
    {
        // Arrange
        var builder = CreateBuilder();

        // Act
        var html = builder.BuildVerificationEmail("TestChild", "token123");

        // Assert
        html.Should().Contain("<a href=\"https://test.mystira.app/privacy\">Privacy Policy</a>");
    }

    #endregion

    #region BuildVerificationEmail - HTML Encoding (XSS Prevention)

    [Fact]
    public void BuildVerificationEmail_HtmlEncodesScriptTagInChildName()
    {
        // Arrange
        var builder = CreateBuilder();

        // Act
        var html = builder.BuildVerificationEmail("<script>alert('xss')</script>", "token123");

        // Assert - the script tag should be HTML-encoded, not rendered as raw HTML
        html.Should().NotContain("<script>");
        html.Should().Contain("&lt;script&gt;");
        html.Should().Contain("&lt;/script&gt;");
    }

    [Fact]
    public void BuildVerificationEmail_HtmlEncodesAmpersandInChildName()
    {
        // Arrange
        var builder = CreateBuilder();

        // Act
        var html = builder.BuildVerificationEmail("Tom & Jerry", "token123");

        // Assert
        html.Should().Contain("Tom &amp; Jerry");
    }

    [Fact]
    public void BuildVerificationEmail_HtmlEncodesQuotesInChildName()
    {
        // Arrange
        var builder = CreateBuilder();

        // Act
        var html = builder.BuildVerificationEmail("Child \"Nickname\" Name", "token123");

        // Assert
        html.Should().Contain("&quot;Nickname&quot;");
    }

    [Fact]
    public void BuildVerificationEmail_HtmlEncodesAngleBracketsInChildName()
    {
        // Arrange
        var builder = CreateBuilder();

        // Act
        var html = builder.BuildVerificationEmail("<b>Bold</b>", "token123");

        // Assert
        html.Should().NotContain("<b>Bold</b>");
        html.Should().Contain("&lt;b&gt;Bold&lt;/b&gt;");
    }

    #endregion

    #region BuildVerificationEmail - HTML Structure

    [Fact]
    public void BuildVerificationEmail_ReturnsValidHtmlDocument()
    {
        // Arrange
        var builder = CreateBuilder();

        // Act
        var html = builder.BuildVerificationEmail("TestChild", "token123");

        // Assert
        html.Should().Contain("<!DOCTYPE html>");
        html.Should().Contain("<html>");
        html.Should().Contain("</html>");
        html.Should().Contain("<body");
        html.Should().Contain("</body>");
    }

    [Fact]
    public void BuildVerificationEmail_ContainsCoppaDisclosure()
    {
        // Arrange
        var builder = CreateBuilder();

        // Act
        var html = builder.BuildVerificationEmail("TestChild", "token123");

        // Assert
        html.Should().Contain("Children's Online Privacy Protection Act (COPPA)");
    }

    [Fact]
    public void BuildVerificationEmail_ContainsExpiryNotice()
    {
        // Arrange
        var builder = CreateBuilder();

        // Act
        var html = builder.BuildVerificationEmail("TestChild", "token123");

        // Assert
        html.Should().Contain("expires in 48 hours");
    }

    #endregion

    #region Helpers

    private static ConsentEmailBuilder CreateBuilder()
    {
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["AppSettings:BaseUrl"] = "https://test.mystira.app"
            })
            .Build();

        return new ConsentEmailBuilder(config);
    }

    #endregion
}
