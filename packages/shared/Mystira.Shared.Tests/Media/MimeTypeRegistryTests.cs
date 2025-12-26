using FluentAssertions;
using Mystira.Shared.Media;
using Xunit;

namespace Mystira.Shared.Tests.Media;

public class MimeTypeRegistryTests
{
    [Theory]
    [InlineData(".jpg", "image/jpeg")]
    [InlineData(".jpeg", "image/jpeg")]
    [InlineData(".png", "image/png")]
    [InlineData(".gif", "image/gif")]
    [InlineData(".pdf", "application/pdf")]
    [InlineData(".json", "application/json")]
    [InlineData(".xml", "application/xml")]
    [InlineData(".txt", "text/plain")]
    [InlineData(".html", "text/html")]
    [InlineData(".css", "text/css")]
    [InlineData(".js", "application/javascript")]
    public void GetMimeType_ReturnsCorrectType_ForKnownExtensions(string extension, string expectedMimeType)
    {
        // Act
        var result = MimeTypeRegistry.GetMimeType(extension);

        // Assert
        result.Should().Be(expectedMimeType);
    }

    [Theory]
    [InlineData("JPG")]
    [InlineData("PNG")]
    [InlineData("PDF")]
    public void GetMimeType_IsCaseInsensitive(string extension)
    {
        // Arrange
        var lowerExt = $".{extension.ToLowerInvariant()}";
        var upperExt = $".{extension.ToUpperInvariant()}";

        // Act
        var lowerResult = MimeTypeRegistry.GetMimeType(lowerExt);
        var upperResult = MimeTypeRegistry.GetMimeType(upperExt);

        // Assert
        lowerResult.Should().Be(upperResult);
    }

    [Fact]
    public void GetMimeType_WithDot_ReturnsCorrectType()
    {
        // Act
        var result = MimeTypeRegistry.GetMimeType(".json");

        // Assert
        result.Should().Be("application/json");
    }

    [Fact]
    public void GetMimeType_WithoutDot_ReturnsCorrectType()
    {
        // Act
        var result = MimeTypeRegistry.GetMimeType("json");

        // Assert
        result.Should().Be("application/json");
    }

    [Fact]
    public void GetMimeType_UnknownExtension_ReturnsOctetStream()
    {
        // Act
        var result = MimeTypeRegistry.GetMimeType(".unknown");

        // Assert
        result.Should().Be("application/octet-stream");
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(null)]
    public void GetMimeType_EmptyOrNullExtension_ReturnsOctetStream(string? extension)
    {
        // Act
        var result = MimeTypeRegistry.GetMimeType(extension!);

        // Assert
        result.Should().Be("application/octet-stream");
    }

    [Theory]
    [InlineData("image/jpeg", ".jpg")]
    [InlineData("image/png", ".png")]
    [InlineData("application/pdf", ".pdf")]
    [InlineData("application/json", ".json")]
    [InlineData("text/plain", ".txt")]
    public void GetExtension_ReturnsCorrectExtension_ForKnownMimeTypes(string mimeType, string expectedExtension)
    {
        // Act
        var result = MimeTypeRegistry.GetExtension(mimeType);

        // Assert
        result.Should().Be(expectedExtension);
    }

    [Fact]
    public void GetExtension_UnknownMimeType_ReturnsEmpty()
    {
        // Act
        var result = MimeTypeRegistry.GetExtension("application/x-unknown");

        // Assert
        result.Should().BeEmpty();
    }

    [Theory]
    [InlineData("IMAGE/JPEG")]
    [InlineData("Image/Jpeg")]
    public void GetExtension_IsCaseInsensitive(string mimeType)
    {
        // Act
        var result = MimeTypeRegistry.GetExtension(mimeType);

        // Assert
        result.Should().Be(".jpg");
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(null)]
    public void GetExtension_EmptyOrNullMimeType_ReturnsEmpty(string? mimeType)
    {
        // Act
        var result = MimeTypeRegistry.GetExtension(mimeType!);

        // Assert
        result.Should().BeEmpty();
    }
}
