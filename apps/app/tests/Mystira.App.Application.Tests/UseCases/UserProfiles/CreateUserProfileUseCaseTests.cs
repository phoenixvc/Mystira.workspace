using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Mystira.Core.Ports.Data;
using Mystira.Core.UseCases.UserProfiles;
using Mystira.Domain.Models;
using Mystira.Domain.Enums;
using Mystira.Domain.ValueObjects;
using Mystira.Contracts.App.Requests.UserProfiles;

namespace Mystira.App.Application.Tests.UseCases.UserProfiles;

public class CreateUserProfileUseCaseTests
{
    private readonly Mock<IUserProfileRepository> _repository;
    private readonly Mock<IUnitOfWork> _unitOfWork;
    private readonly Mock<ILogger<CreateUserProfileUseCase>> _logger;
    private readonly CreateUserProfileUseCase _useCase;

    public CreateUserProfileUseCaseTests()
    {
        _repository = new Mock<IUserProfileRepository>();
        _unitOfWork = new Mock<IUnitOfWork>();
        _logger = new Mock<ILogger<CreateUserProfileUseCase>>();
        _useCase = new CreateUserProfileUseCase(_repository.Object, _unitOfWork.Object, _logger.Object);
    }

    [Fact]
    public async Task ExecuteAsync_WithValidRequest_CreatesProfile()
    {
        var request = new CreateUserProfileRequest
        {
            Name = "Player One",
            AccountId = "acc-1",
            AgeGroup = "middle_childhood",
            IsGuest = false,
            IsNpc = false,
            HasCompletedOnboarding = true
        };

        var result = await _useCase.ExecuteAsync(request);

        result.Should().NotBeNull();
        result.Name.Should().Be("Player One");
        result.Id.Should().NotBeNullOrEmpty();
        _repository.Verify(r => r.AddAsync(It.IsAny<UserProfile>(), It.IsAny<CancellationToken>()), Times.Once);
        _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ExecuteAsync_WithGuestProfile_SetsIsGuestTrue()
    {
        var request = new CreateUserProfileRequest
        {
            Name = "Guest",
            AccountId = "acc-1",
            AgeGroup = "middle_childhood",
            IsGuest = true
        };

        var result = await _useCase.ExecuteAsync(request);

        result.IsGuest.Should().BeTrue();
    }

    [Fact]
    public async Task ExecuteAsync_WithNullRequest_ThrowsNullReferenceException()
    {
        var act = () => _useCase.ExecuteAsync(null!);

        await act.Should().ThrowAsync<NullReferenceException>();
    }

    [Fact]
    public async Task ExecuteAsync_WithEmptyName_CreatesProfile()
    {
        var request = new CreateUserProfileRequest
        {
            Name = "",
            AccountId = "acc-1",
            AgeGroup = "middle_childhood"
        };

        var result = await _useCase.ExecuteAsync(request);

        result.Should().NotBeNull();
        result.Name.Should().BeEmpty();
    }
}
