using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Mystira.Application.Ports.Data;
using Mystira.App.Application.UseCases.UserProfiles;
using Mystira.Domain.Models;
using Mystira.Domain.Enums;
using Mystira.Domain.ValueObjects;
using Mystira.Contracts.App.Requests.UserProfiles;

namespace Mystira.App.Application.Tests.UseCases.UserProfiles;

public class UpdateUserProfileUseCaseTests
{
    private readonly Mock<IUserProfileRepository> _repository;
    private readonly Mock<IUnitOfWork> _unitOfWork;
    private readonly Mock<ILogger<UpdateUserProfileUseCase>> _logger;
    private readonly UpdateUserProfileUseCase _useCase;

    public UpdateUserProfileUseCaseTests()
    {
        _repository = new Mock<IUserProfileRepository>();
        _unitOfWork = new Mock<IUnitOfWork>();
        _logger = new Mock<ILogger<UpdateUserProfileUseCase>>();
        _useCase = new UpdateUserProfileUseCase(_repository.Object, _unitOfWork.Object, _logger.Object);
    }

    [Fact]
    public async Task ExecuteAsync_WithValidRequest_UpdatesProfile()
    {
        var profile = new UserProfile { Id = "p1", Name = "Old Name" };
        _repository.Setup(r => r.GetByIdAsync("p1", It.IsAny<CancellationToken>()))
            .ReturnsAsync(profile);

        var request = new UpdateUserProfileRequest
        {
            AgeGroup = "preteen",
            HasCompletedOnboarding = true
        };

        var result = await _useCase.ExecuteAsync("p1", request);

        result.Should().NotBeNull();
        _repository.Verify(r => r.UpdateAsync(It.IsAny<UserProfile>(), It.IsAny<CancellationToken>()), Times.Once);
        _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ExecuteAsync_WithNonExistingProfile_ReturnsNull()
    {
        _repository.Setup(r => r.GetByIdAsync("missing", It.IsAny<CancellationToken>()))
            .ReturnsAsync(default(UserProfile));

        var result = await _useCase.ExecuteAsync("missing", new UpdateUserProfileRequest());

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

        var result = await _useCase.ExecuteAsync(id!, new UpdateUserProfileRequest());

        result.Should().BeNull();
    }

    [Fact]
    public async Task ExecuteAsync_WithNullRequest_ReturnsNull()
    {
        var result = await _useCase.ExecuteAsync("p1", null!);

        result.Should().BeNull();
    }
}
