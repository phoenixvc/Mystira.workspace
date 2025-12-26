using FluentAssertions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Mystira.Shared.Authentication;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Xunit;

namespace Mystira.Shared.Tests.Authentication;

public class LocalJwtServiceTests
{
    private readonly Mock<ILogger<LocalJwtService>> _loggerMock;
    private readonly LocalJwtOptions _options;
    private readonly LocalJwtService _sut;

    public LocalJwtServiceTests()
    {
        _loggerMock = new Mock<ILogger<LocalJwtService>>();
        _options = new LocalJwtOptions
        {
            Issuer = "TestIssuer",
            Audience = "TestAudience",
            SecretKey = "ThisIsAVeryLongSecretKeyForTesting12345678901234567890", // At least 32 chars
            AccessTokenExpirationHours = 1,
            ClockSkewMinutes = 1
        };
        _sut = new LocalJwtService(_options, _loggerMock.Object);
    }

    [Fact]
    public void GenerateAccessToken_WithUserDetails_ReturnsValidToken()
    {
        // Act
        var token = _sut.GenerateAccessToken("user123", "test@example.com", "Test User", "Admin");

        // Assert
        token.Should().NotBeNullOrWhiteSpace();
        var handler = new JwtSecurityTokenHandler();
        var jwtToken = handler.ReadJwtToken(token);
        
        // Check for claims by type string as they appear in the JWT
        var claims = jwtToken.Claims.ToList();
        claims.Should().Contain(c => c.Type == "nameid" && c.Value == "user123");
        claims.Should().Contain(c => c.Type == "email" && c.Value == "test@example.com");
        claims.Should().Contain(c => c.Type == "unique_name" && c.Value == "Test User");
        claims.Should().Contain(c => c.Type == "role" && c.Value == "Admin");
    }

    [Fact]
    public void GenerateAccessToken_WithoutRole_AddsDefaultGuestRole()
    {
        // Act
        var token = _sut.GenerateAccessToken("user123", "test@example.com", "Test User");

        // Assert
        var handler = new JwtSecurityTokenHandler();
        var jwtToken = handler.ReadJwtToken(token);
        jwtToken.Claims.Should().Contain(c => c.Type == "role" && c.Value == "Guest");
    }

    [Fact]
    public void GenerateAccessToken_WithCustomClaims_IncludesAllClaims()
    {
        // Arrange
        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, "user456"),
            new Claim(ClaimTypes.Email, "custom@example.com"),
            new Claim("CustomClaim", "CustomValue")
        };

        // Act
        var token = _sut.GenerateAccessToken(claims);

        // Assert
        token.Should().NotBeNullOrWhiteSpace();
        var handler = new JwtSecurityTokenHandler();
        var jwtToken = handler.ReadJwtToken(token);
        
        // Check for claims that should be there (using short form names as they appear in JWT)
        var tokenClaims = jwtToken.Claims.ToList();
        tokenClaims.Should().Contain(c => c.Type == "nameid" && c.Value == "user456");
        tokenClaims.Should().Contain(c => c.Type == "CustomClaim" && c.Value == "CustomValue");
    }

    [Fact]
    public void GenerateAccessToken_SetsCorrectIssuerAndAudience()
    {
        // Act
        var token = _sut.GenerateAccessToken("user123", "test@example.com", "Test User");

        // Assert
        var handler = new JwtSecurityTokenHandler();
        var jwtToken = handler.ReadJwtToken(token);
        jwtToken.Issuer.Should().Be("TestIssuer");
        jwtToken.Audiences.Should().Contain("TestAudience");
    }

    [Fact]
    public void GenerateAccessToken_SetsExpirationTime()
    {
        // Act
        var token = _sut.GenerateAccessToken("user123", "test@example.com", "Test User");

        // Assert
        var handler = new JwtSecurityTokenHandler();
        var jwtToken = handler.ReadJwtToken(token);
        jwtToken.ValidTo.Should().BeCloseTo(DateTime.UtcNow.AddHours(1), TimeSpan.FromMinutes(1));
    }

    [Fact]
    public void GenerateRefreshToken_ReturnsNonEmptyString()
    {
        // Act
        var token = _sut.GenerateRefreshToken();

        // Assert
        token.Should().NotBeNullOrWhiteSpace();
        token.Length.Should().BeGreaterThan(32); // Base64 encoded bytes
    }

    [Fact]
    public void GenerateRefreshToken_GeneratesUniqueTokens()
    {
        // Act
        var token1 = _sut.GenerateRefreshToken();
        var token2 = _sut.GenerateRefreshToken();

        // Assert
        token1.Should().NotBe(token2);
    }

    [Fact]
    public void ValidateToken_WithValidToken_ReturnsTrue()
    {
        // Arrange
        var token = _sut.GenerateAccessToken("user123", "test@example.com", "Test User");

        // Act
        var isValid = _sut.ValidateToken(token);

        // Assert
        isValid.Should().BeTrue();
    }

    [Fact]
    public void ValidateToken_WithInvalidToken_ReturnsFalse()
    {
        // Arrange
        var invalidToken = "invalid.token.string";

        // Act
        var isValid = _sut.ValidateToken(invalidToken);

        // Assert
        isValid.Should().BeFalse();
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(null)]
    public void ValidateToken_WithEmptyOrNullToken_ReturnsFalse(string? token)
    {
        // Act
        var isValid = _sut.ValidateToken(token!);

        // Assert
        isValid.Should().BeFalse();
    }

    [Fact]
    public void GetUserIdFromToken_WithValidToken_ReturnsUserId()
    {
        // Arrange
        var expectedUserId = "user123";
        var token = _sut.GenerateAccessToken(expectedUserId, "test@example.com", "Test User");

        // Act
        var userId = _sut.GetUserIdFromToken(token);

        // Assert
        userId.Should().Be(expectedUserId);
    }

    [Fact]
    public void GetUserIdFromToken_WithInvalidToken_ReturnsNull()
    {
        // Arrange
        var invalidToken = "invalid.token.string";

        // Act
        var userId = _sut.GetUserIdFromToken(invalidToken);

        // Assert
        userId.Should().BeNull();
    }

    [Fact]
    public void ValidateAndExtractUserId_WithValidToken_ReturnsValidAndUserId()
    {
        // Arrange
        var expectedUserId = "user123";
        var token = _sut.GenerateAccessToken(expectedUserId, "test@example.com", "Test User");

        // Act
        var (isValid, userId) = _sut.ValidateAndExtractUserId(token);

        // Assert
        isValid.Should().BeTrue();
        userId.Should().Be(expectedUserId);
    }

    [Fact]
    public void ValidateAndExtractUserId_WithInvalidToken_ReturnsInvalidAndNull()
    {
        // Arrange
        var invalidToken = "invalid.token.string";

        // Act
        var (isValid, userId) = _sut.ValidateAndExtractUserId(invalidToken);

        // Assert
        isValid.Should().BeFalse();
        userId.Should().BeNull();
    }

    [Fact]
    public void ValidateRefreshToken_WithMatchingTokens_ReturnsTrue()
    {
        // Arrange
        var token = "matching-refresh-token";

        // Act
        var isValid = _sut.ValidateRefreshToken(token, token);

        // Assert
        isValid.Should().BeTrue();
    }

    [Fact]
    public void ValidateRefreshToken_WithNonMatchingTokens_ReturnsFalse()
    {
        // Arrange
        var token1 = "refresh-token-1";
        var token2 = "refresh-token-2";

        // Act
        var isValid = _sut.ValidateRefreshToken(token1, token2);

        // Assert
        isValid.Should().BeFalse();
    }

    [Theory]
    [InlineData("", "token")]
    [InlineData("token", "")]
    [InlineData(null, "token")]
    [InlineData("token", null)]
    public void ValidateRefreshToken_WithEmptyOrNullTokens_ReturnsFalse(string? token, string? storedToken)
    {
        // Act
        var isValid = _sut.ValidateRefreshToken(token!, storedToken!);

        // Assert
        isValid.Should().BeFalse();
    }

    [Fact]
    public void ValidateRefreshToken_IsCaseSensitive()
    {
        // Arrange
        var token = "RefreshToken123";
        var storedToken = "refreshtoken123";

        // Act
        var isValid = _sut.ValidateRefreshToken(token, storedToken);

        // Assert
        isValid.Should().BeFalse();
    }

    [Fact]
    public void ValidateAndGetPrincipal_WithValidToken_ReturnsPrincipal()
    {
        // Arrange
        var expectedUserId = "user123";
        var token = _sut.GenerateAccessToken(expectedUserId, "test@example.com", "Test User");

        // Act
        var principal = _sut.ValidateAndGetPrincipal(token);

        // Assert
        principal.Should().NotBeNull();
        principal!.FindFirst(ClaimTypes.NameIdentifier)?.Value.Should().Be(expectedUserId);
    }

    [Fact]
    public void ValidateAndGetPrincipal_WithInvalidToken_ReturnsNull()
    {
        // Arrange
        var invalidToken = "invalid.token.string";

        // Act
        var principal = _sut.ValidateAndGetPrincipal(invalidToken);

        // Assert
        principal.Should().BeNull();
    }
}
