using Mystira.Shared.Exceptions;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Mystira.App.Application.Ports.Data;
using Mystira.App.Application.UseCases.Accounts;
using Mystira.App.Domain.Models;
using Mystira.Shared.Data.Repositories;

namespace Mystira.App.Application.Tests.UseCases;

public class AddUserProfileToAccountUseCaseTests
{
    private readonly Mock<IAccountRepository> _accountRepository;
    private readonly Mock<IUserProfileRepository> _profileRepository;
    private readonly Mock<IUnitOfWork> _unitOfWork;
    private readonly Mock<ILogger<AddUserProfileToAccountUseCase>> _logger;
    private readonly AddUserProfileToAccountUseCase _useCase;

    public AddUserProfileToAccountUseCaseTests()
    {
        _accountRepository = new Mock<IAccountRepository>();
        _profileRepository = new Mock<IUserProfileRepository>();
        _unitOfWork = new Mock<IUnitOfWork>();
        _logger = new Mock<ILogger<AddUserProfileToAccountUseCase>>();

        _useCase = new AddUserProfileToAccountUseCase(
            _accountRepository.Object, _profileRepository.Object,
            _unitOfWork.Object, _logger.Object);
    }

    [Fact]
    public async Task ExecuteAsync_WithValidIds_LinksProfileToAccount()
    {
        var account = new Account { Id = "acc-1", UserProfileIds = new List<string>() };
        var profile = new UserProfile { Id = "profile-1", AccountId = null };

        _accountRepository.Setup(r => r.GetByIdAsync("acc-1", It.IsAny<CancellationToken>()))
            .ReturnsAsync(account);
        _profileRepository.Setup(r => r.GetByIdAsync("profile-1", It.IsAny<CancellationToken>()))
            .ReturnsAsync(profile);

        var result = await _useCase.ExecuteAsync("acc-1", "profile-1");

        result.Should().NotBeNull();
        profile.AccountId.Should().Be("acc-1");
        account.UserProfileIds.Should().Contain("profile-1");

        _profileRepository.Verify(r => r.UpdateAsync(profile, It.IsAny<CancellationToken>()), Times.Once);
        _accountRepository.Verify(r => r.UpdateAsync(account, It.IsAny<CancellationToken>()), Times.Once);
        _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ExecuteAsync_WithAlreadyLinkedProfile_ReturnsAccountWithoutUpdate()
    {
        var account = new Account { Id = "acc-1", UserProfileIds = new List<string> { "profile-1" } };
        var profile = new UserProfile { Id = "profile-1", AccountId = "acc-1" };

        _accountRepository.Setup(r => r.GetByIdAsync("acc-1", It.IsAny<CancellationToken>()))
            .ReturnsAsync(account);
        _profileRepository.Setup(r => r.GetByIdAsync("profile-1", It.IsAny<CancellationToken>()))
            .ReturnsAsync(profile);

        var result = await _useCase.ExecuteAsync("acc-1", "profile-1");

        result.Should().BeSameAs(account);
        _profileRepository.Verify(r => r.UpdateAsync(It.IsAny<UserProfile>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Theory]
    [InlineData(null, "profile-1")]
    [InlineData("", "profile-1")]
    [InlineData("acc-1", null)]
    [InlineData("acc-1", "")]
    public async Task ExecuteAsync_WithNullOrEmptyIds_ThrowsValidationException(string? accountId, string? profileId)
    {
        var act = () => _useCase.ExecuteAsync(accountId!, profileId!);

        await act.Should().ThrowAsync<ValidationException>();
    }

    [Fact]
    public async Task ExecuteAsync_WithMissingAccount_ThrowsValidationException()
    {
        _accountRepository.Setup(r => r.GetByIdAsync("missing", It.IsAny<CancellationToken>()))
            .ReturnsAsync(default(Account));

        var act = () => _useCase.ExecuteAsync("missing", "profile-1");

        await act.Should().ThrowAsync<ValidationException>()
            .WithMessage("*Account not found*");
    }

    [Fact]
    public async Task ExecuteAsync_WithMissingProfile_ThrowsValidationException()
    {
        var account = new Account { Id = "acc-1", UserProfileIds = new List<string>() };
        _accountRepository.Setup(r => r.GetByIdAsync("acc-1", It.IsAny<CancellationToken>()))
            .ReturnsAsync(account);
        _profileRepository.Setup(r => r.GetByIdAsync("missing", It.IsAny<CancellationToken>()))
            .ReturnsAsync(default(UserProfile));

        var act = () => _useCase.ExecuteAsync("acc-1", "missing");

        await act.Should().ThrowAsync<ValidationException>()
            .WithMessage("*User profile not found*");
    }

    [Fact]
    public async Task ExecuteAsync_WithProfileAlreadyInList_DoesNotDuplicateInList()
    {
        var account = new Account { Id = "acc-1", UserProfileIds = new List<string> { "profile-1" } };
        var profile = new UserProfile { Id = "profile-1", AccountId = "other-account" };

        _accountRepository.Setup(r => r.GetByIdAsync("acc-1", It.IsAny<CancellationToken>()))
            .ReturnsAsync(account);
        _profileRepository.Setup(r => r.GetByIdAsync("profile-1", It.IsAny<CancellationToken>()))
            .ReturnsAsync(profile);

        await _useCase.ExecuteAsync("acc-1", "profile-1");

        account.UserProfileIds.Should().HaveCount(1);
        _accountRepository.Verify(r => r.UpdateAsync(account, It.IsAny<CancellationToken>()), Times.Never);
    }
}
