using Mystira.Shared.Exceptions;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Mystira.Core.Ports.Data;
using Mystira.Core.UseCases.Badges;
using Mystira.Domain.Models;
using Mystira.Domain.Enums;
using Mystira.Domain.ValueObjects;

namespace Mystira.App.Application.Tests.UseCases.Badges;

public class GetBadgesByAxisUseCaseTests
{
    private readonly Mock<IUserBadgeRepository> _repository;
    private readonly Mock<ILogger<GetBadgesByAxisUseCase>> _logger;
    private readonly GetBadgesByAxisUseCase _useCase;

    public GetBadgesByAxisUseCaseTests()
    {
        _repository = new Mock<IUserBadgeRepository>();
        _logger = new Mock<ILogger<GetBadgesByAxisUseCase>>();
        _useCase = new GetBadgesByAxisUseCase(_repository.Object, _logger.Object);
    }

    [Fact]
    public async Task ExecuteAsync_WithValidInput_ReturnsBadgesForAxis()
    {
        var badges = new List<UserBadge>
        {
            new() { Id = "b1", Axis = "courage", UserProfileId = "profile-1" },
            new() { Id = "b2", Axis = "courage", UserProfileId = "profile-1" }
        };
        _repository.Setup(r => r.GetByUserProfileIdAndAxisAsync("profile-1", "courage", It.IsAny<CancellationToken>()))
            .ReturnsAsync(badges);

        var result = await _useCase.ExecuteAsync("profile-1", "courage");

        result.Should().HaveCount(2);
        result.Should().OnlyContain(b => b.Axis == "courage");
    }

    [Fact]
    public async Task ExecuteAsync_WithNoMatches_ReturnsEmptyList()
    {
        _repository.Setup(r => r.GetByUserProfileIdAndAxisAsync("profile-1", "honesty", It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<UserBadge>());

        var result = await _useCase.ExecuteAsync("profile-1", "honesty");

        result.Should().BeEmpty();
    }

    [Theory]
    [InlineData(null, "courage")]
    [InlineData("", "courage")]
    [InlineData("profile-1", null)]
    [InlineData("profile-1", "")]
    public async Task ExecuteAsync_WithNullOrEmptyInputs_ThrowsValidationException(string? profileId, string? axis)
    {
        var act = () => _useCase.ExecuteAsync(profileId!, axis!);

        await act.Should().ThrowAsync<ValidationException>();
    }
}
