using Mystira.Shared.Exceptions;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Mystira.Core.Ports.Data;
using Mystira.App.Application.UseCases.Badges;
using Mystira.Domain.Models;
using Mystira.Domain.Enums;
using Mystira.Domain.ValueObjects;

namespace Mystira.App.Application.Tests.UseCases.Badges;

public class GetBadgeUseCaseTests
{
    private readonly Mock<IUserBadgeRepository> _repository;
    private readonly Mock<ILogger<GetBadgeUseCase>> _logger;
    private readonly GetBadgeUseCase _useCase;

    public GetBadgeUseCaseTests()
    {
        _repository = new Mock<IUserBadgeRepository>();
        _logger = new Mock<ILogger<GetBadgeUseCase>>();
        _useCase = new GetBadgeUseCase(_repository.Object, _logger.Object);
    }

    [Fact]
    public async Task ExecuteAsync_WithExistingId_ReturnsBadge()
    {
        var badge = new UserBadge { Id = "badge-1", BadgeName = "Courage Master", Axis = "courage" };
        _repository.Setup(r => r.GetByIdAsync("badge-1", It.IsAny<CancellationToken>()))
            .ReturnsAsync(badge);

        var result = await _useCase.ExecuteAsync("badge-1");

        result.Should().NotBeNull();
        result!.Id.Should().Be("badge-1");
        result.BadgeName.Should().Be("Courage Master");
    }

    [Fact]
    public async Task ExecuteAsync_WithNonExistingId_ReturnsNull()
    {
        _repository.Setup(r => r.GetByIdAsync("missing", It.IsAny<CancellationToken>()))
            .ReturnsAsync(default(UserBadge));

        var result = await _useCase.ExecuteAsync("missing");

        result.Should().BeNull();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public async Task ExecuteAsync_WithNullOrEmptyId_ThrowsValidationException(string? badgeId)
    {
        var act = () => _useCase.ExecuteAsync(badgeId!);

        await act.Should().ThrowAsync<ValidationException>();
    }
}
