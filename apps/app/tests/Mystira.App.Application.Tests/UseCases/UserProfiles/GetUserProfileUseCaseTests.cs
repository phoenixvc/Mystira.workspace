using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Mystira.App.Application.Ports.Data;
using Mystira.App.Application.UseCases.UserProfiles;
using Mystira.Domain.Models;
using Mystira.Domain.Enums;
using Mystira.Domain.ValueObjects;

namespace Mystira.App.Application.Tests.UseCases.UserProfiles;

public class GetUserProfileUseCaseTests
{
    private readonly Mock<IUserProfileRepository> _repository;
    private readonly Mock<ILogger<GetUserProfileUseCase>> _logger;
    private readonly GetUserProfileUseCase _useCase;

    public GetUserProfileUseCaseTests()
    {
        _repository = new Mock<IUserProfileRepository>();
        _logger = new Mock<ILogger<GetUserProfileUseCase>>();
        _useCase = new GetUserProfileUseCase(_repository.Object, _logger.Object);
    }

    [Fact]
    public async Task ExecuteAsync_WithExistingId_ReturnsProfile()
    {
        var profile = new UserProfile { Id = "p1", Name = "Player One" };
        _repository.Setup(r => r.GetByIdAsync("p1", It.IsAny<CancellationToken>()))
            .ReturnsAsync(profile);

        var result = await _useCase.ExecuteAsync("p1");

        result.Should().NotBeNull();
        result!.Id.Should().Be("p1");
        result.Name.Should().Be("Player One");
    }

    [Fact]
    public async Task ExecuteAsync_WithNonExistingId_ReturnsNull()
    {
        _repository.Setup(r => r.GetByIdAsync("missing", It.IsAny<CancellationToken>()))
            .ReturnsAsync(default(UserProfile));

        var result = await _useCase.ExecuteAsync("missing");

        result.Should().BeNull();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public async Task ExecuteAsync_WithNullOrEmptyId_ReturnsNull(string? id)
    {
        _repository.Setup(r => r.GetByIdAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(default(UserProfile));

        var result = await _useCase.ExecuteAsync(id!);

        result.Should().BeNull();
    }
}
