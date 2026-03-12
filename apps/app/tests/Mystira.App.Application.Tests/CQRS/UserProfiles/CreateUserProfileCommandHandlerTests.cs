using Mystira.Shared.Exceptions;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Mystira.Core.CQRS.UserProfiles.Commands;
using Mystira.Core.Ports.Data;
using Mystira.Core.UseCases.UserProfiles;
using Mystira.Domain.Models;
using Mystira.Domain.Enums;
using Mystira.Domain.ValueObjects;
using Mystira.Contracts.App.Requests.UserProfiles;
using Mystira.Shared.Data.Repositories;

namespace Mystira.App.Application.Tests.CQRS.UserProfiles;

public class CreateUserProfileCommandHandlerTests
{
    private readonly Mock<IUserProfileRepository> _repository;
    private readonly Mock<IUnitOfWork> _unitOfWork;
    private readonly Mock<ILogger<CreateUserProfileUseCase>> _logger;

    public CreateUserProfileCommandHandlerTests()
    {
        _repository = new Mock<IUserProfileRepository>();
        _unitOfWork = new Mock<IUnitOfWork>();
        _logger = new Mock<ILogger<CreateUserProfileUseCase>>();
    }

    [Fact]
    public async Task Handle_WithValidRequest_CreatesNewProfile()
    {
        // Arrange
        var request = new CreateUserProfileRequest
        {
            Name = "Test Profile",
            AgeGroup = AgeGroupConstants.MiddleChildhood,
            AccountId = "acc-123"
        };
        var command = new CreateUserProfileCommand(request);
        var useCase = new CreateUserProfileUseCase(_repository.Object, _unitOfWork.Object, _logger.Object);

        // Act
        var result = await CreateUserProfileCommandHandler.Handle(
            command, useCase, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Name.Should().Be("Test Profile");
        result.AgeGroupId.Should().Be(AgeGroupConstants.MiddleChildhood);
        result.AccountId.Should().Be("acc-123");
        result.Id.Should().NotBeNullOrEmpty();

        _repository.Verify(r => r.AddAsync(It.IsAny<UserProfile>(), It.IsAny<CancellationToken>()), Times.Once);
        _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WithEmptyName_ThrowsValidationException()
    {
        // Arrange
        var request = new CreateUserProfileRequest
        {
            Name = "",
            AgeGroup = AgeGroupConstants.MiddleChildhood
        };
        var command = new CreateUserProfileCommand(request);
        var useCase = new CreateUserProfileUseCase(_repository.Object, _unitOfWork.Object, _logger.Object);

        // Act
        var act = () => CreateUserProfileCommandHandler.Handle(
            command, useCase, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<ValidationException>()
            .WithMessage("*name is required*");
    }

    [Fact]
    public async Task Handle_WithNullName_ThrowsValidationException()
    {
        // Arrange
        var request = new CreateUserProfileRequest
        {
            Name = null!,
            AgeGroup = AgeGroupConstants.MiddleChildhood
        };
        var command = new CreateUserProfileCommand(request);
        var useCase = new CreateUserProfileUseCase(_repository.Object, _unitOfWork.Object, _logger.Object);

        // Act
        var act = () => CreateUserProfileCommandHandler.Handle(
            command, useCase, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<ValidationException>()
            .WithMessage("*name is required*");
    }

    [Fact]
    public async Task Handle_WithShortName_ThrowsValidationException()
    {
        // Arrange
        var request = new CreateUserProfileRequest
        {
            Name = "A",
            AgeGroup = AgeGroupConstants.MiddleChildhood
        };
        var command = new CreateUserProfileCommand(request);
        var useCase = new CreateUserProfileUseCase(_repository.Object, _unitOfWork.Object, _logger.Object);

        // Act
        var act = () => CreateUserProfileCommandHandler.Handle(
            command, useCase, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<ValidationException>()
            .WithMessage("*at least 2 characters*");
    }

    [Fact]
    public async Task Handle_WithEmptyAgeGroup_ThrowsValidationException()
    {
        // Arrange - UseCase validates age group
        var request = new CreateUserProfileRequest
        {
            Name = "Valid Name",
            AgeGroup = ""
        };
        var command = new CreateUserProfileCommand(request);
        var useCase = new CreateUserProfileUseCase(_repository.Object, _unitOfWork.Object, _logger.Object);

        // Act
        var act = () => CreateUserProfileCommandHandler.Handle(
            command, useCase, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<ValidationException>()
            .WithMessage("*Invalid age group*");
    }

    [Fact]
    public async Task Handle_WithInvalidAgeGroup_ThrowsValidationException()
    {
        // Arrange
        var request = new CreateUserProfileRequest
        {
            Name = "Valid Name",
            AgeGroup = "invalid-group"
        };
        var command = new CreateUserProfileCommand(request);
        var useCase = new CreateUserProfileUseCase(_repository.Object, _unitOfWork.Object, _logger.Object);

        // Act
        var act = () => CreateUserProfileCommandHandler.Handle(
            command, useCase, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<ValidationException>()
            .WithMessage("*Invalid age group*");
    }

    [Theory]
    [InlineData(AgeGroupConstants.EarlyChildhood)]
    [InlineData(AgeGroupConstants.MiddleChildhood)]
    [InlineData(AgeGroupConstants.Preteen)]
    [InlineData(AgeGroupConstants.Teen)]
    [InlineData(AgeGroupConstants.Adult)]
    public async Task Handle_WithValidAgeGroups_CreatesProfile(string ageGroup)
    {
        // Arrange
        var request = new CreateUserProfileRequest
        {
            Name = "Test Profile",
            AgeGroup = ageGroup
        };
        var command = new CreateUserProfileCommand(request);
        var useCase = new CreateUserProfileUseCase(_repository.Object, _unitOfWork.Object, _logger.Object);

        // Act
        var result = await CreateUserProfileCommandHandler.Handle(
            command, useCase, CancellationToken.None);

        // Assert
        result.AgeGroupId.Should().Be(ageGroup);
    }

    [Fact]
    public async Task Handle_WithDateOfBirth_UpdatesAgeGroupFromBirthDate()
    {
        // Arrange
        var birthDate = DateTime.Today.AddYears(-7); // 7 years old -> middle_childhood age group
        var request = new CreateUserProfileRequest
        {
            Name = "Child Profile",
            AgeGroup = AgeGroupConstants.EarlyChildhood, // Initial age group (will be updated by UseCase)
            DateOfBirth = birthDate
        };
        var command = new CreateUserProfileCommand(request);
        var useCase = new CreateUserProfileUseCase(_repository.Object, _unitOfWork.Object, _logger.Object);

        // Act
        var result = await CreateUserProfileCommandHandler.Handle(
            command, useCase, CancellationToken.None);

        // Assert
        result.DateOfBirth.Should().Be(DateOnly.FromDateTime(birthDate));
        result.AgeGroupId.Should().Be(AgeGroupConstants.MiddleChildhood);
    }

    [Fact]
    public async Task Handle_WithGuestProfile_SetsIsGuestTrue()
    {
        // Arrange
        var request = new CreateUserProfileRequest
        {
            Name = "Guest User",
            AgeGroup = AgeGroupConstants.MiddleChildhood,
            IsGuest = true
        };
        var command = new CreateUserProfileCommand(request);
        var useCase = new CreateUserProfileUseCase(_repository.Object, _unitOfWork.Object, _logger.Object);

        // Act
        var result = await CreateUserProfileCommandHandler.Handle(
            command, useCase, CancellationToken.None);

        // Assert
        result.IsGuest.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_WithNpcProfile_SetsIsNpcTrue()
    {
        // Arrange
        var request = new CreateUserProfileRequest
        {
            Name = "NPC Character",
            AgeGroup = AgeGroupConstants.Preteen,
            IsNpc = true
        };
        var command = new CreateUserProfileCommand(request);
        var useCase = new CreateUserProfileUseCase(_repository.Object, _unitOfWork.Object, _logger.Object);

        // Act
        var result = await CreateUserProfileCommandHandler.Handle(
            command, useCase, CancellationToken.None);

        // Assert
        result.IsNpc.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_WithPronouns_SetsPronouns()
    {
        // Arrange
        var request = new CreateUserProfileRequest
        {
            Name = "Pronoun User",
            AgeGroup = AgeGroupConstants.Teen,
            Pronouns = "they/them"
        };
        var command = new CreateUserProfileCommand(request);
        var useCase = new CreateUserProfileUseCase(_repository.Object, _unitOfWork.Object, _logger.Object);

        // Act
        var result = await CreateUserProfileCommandHandler.Handle(
            command, useCase, CancellationToken.None);

        // Assert
        result.Pronouns.Should().Be("they/them");
    }

    [Fact]
    public async Task Handle_WithBio_SetsBio()
    {
        // Arrange
        var request = new CreateUserProfileRequest
        {
            Name = "Bio User",
            AgeGroup = AgeGroupConstants.Adult,
            Bio = "Hello, I love adventures!"
        };
        var command = new CreateUserProfileCommand(request);
        var useCase = new CreateUserProfileUseCase(_repository.Object, _unitOfWork.Object, _logger.Object);

        // Act
        var result = await CreateUserProfileCommandHandler.Handle(
            command, useCase, CancellationToken.None);

        // Assert
        result.Bio.Should().Be("Hello, I love adventures!");
    }

    [Fact]
    public async Task Handle_WithAvatar_SetsAvatarMediaId()
    {
        // Arrange
        var request = new CreateUserProfileRequest
        {
            Name = "Avatar User",
            AgeGroup = AgeGroupConstants.MiddleChildhood,
            SelectedAvatarMediaId = "avatar-123"
        };
        var command = new CreateUserProfileCommand(request);
        var useCase = new CreateUserProfileUseCase(_repository.Object, _unitOfWork.Object, _logger.Object);

        // Act
        var result = await CreateUserProfileCommandHandler.Handle(
            command, useCase, CancellationToken.None);

        // Assert
        result.SelectedAvatarMediaId.Should().Be("avatar-123");
        result.AvatarMediaId.Should().Be("avatar-123");
    }

    [Fact]
    public async Task Handle_WithCompletedOnboarding_SetsFlag()
    {
        // Arrange
        var request = new CreateUserProfileRequest
        {
            Name = "Onboarded User",
            AgeGroup = AgeGroupConstants.Preteen,
            HasCompletedOnboarding = true
        };
        var command = new CreateUserProfileCommand(request);
        var useCase = new CreateUserProfileUseCase(_repository.Object, _unitOfWork.Object, _logger.Object);

        // Act
        var result = await CreateUserProfileCommandHandler.Handle(
            command, useCase, CancellationToken.None);

        // Assert
        result.HasCompletedOnboarding.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_SetsCreatedAtAndUpdatedAt()
    {
        // Arrange
        var request = new CreateUserProfileRequest
        {
            Name = "Timestamp User",
            AgeGroup = AgeGroupConstants.MiddleChildhood
        };
        var command = new CreateUserProfileCommand(request);
        var useCase = new CreateUserProfileUseCase(_repository.Object, _unitOfWork.Object, _logger.Object);

        // Act
        var result = await CreateUserProfileCommandHandler.Handle(
            command, useCase, CancellationToken.None);

        // Assert
        result.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
        result.UpdatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public async Task Handle_GeneratesIdWhenNotProvided()
    {
        // Arrange
        var request = new CreateUserProfileRequest
        {
            Name = "No ID User",
            AgeGroup = AgeGroupConstants.MiddleChildhood
        };
        var command = new CreateUserProfileCommand(request);
        var useCase = new CreateUserProfileUseCase(_repository.Object, _unitOfWork.Object, _logger.Object);

        // Act
        var result = await CreateUserProfileCommandHandler.Handle(
            command, useCase, CancellationToken.None);

        // Assert
        result.Id.Should().NotBeNullOrEmpty();
        Guid.TryParse(result.Id, out _).Should().BeTrue();
    }
}
