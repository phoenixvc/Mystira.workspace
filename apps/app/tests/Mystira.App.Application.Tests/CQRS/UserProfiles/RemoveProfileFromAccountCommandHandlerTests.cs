using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Mystira.App.Application.CQRS.UserProfiles.Commands;
using Mystira.Application.Ports.Data;
using Mystira.Domain.Models;
using Mystira.Domain.Enums;
using Mystira.Domain.ValueObjects;
using Mystira.Shared.Data.Repositories;

namespace Mystira.App.Application.Tests.CQRS.UserProfiles;

public class RemoveProfileFromAccountCommandHandlerTests
{
    private readonly Mock<IUserProfileRepository> _profileRepository;
    private readonly Mock<IAccountRepository> _accountRepository;
    private readonly Mock<IUnitOfWork> _unitOfWork;
    private readonly Mock<ILogger> _logger;

    public RemoveProfileFromAccountCommandHandlerTests()
    {
        _profileRepository = new Mock<IUserProfileRepository>();
        _accountRepository = new Mock<IAccountRepository>();
        _unitOfWork = new Mock<IUnitOfWork>();
        _logger = new Mock<ILogger>();
    }

    [Fact]
    public async Task Handle_WithLinkedProfile_UnlinksFromAccount()
    {
        var profile = new UserProfile { Id = "profile-1", AccountId = "account-1" };
        var account = new Account
        {
            Id = "account-1",
            UserProfileIds = new List<string> { "profile-1", "profile-2" }
        };
        var command = new RemoveProfileFromAccountCommand("profile-1");

        _profileRepository.Setup(r => r.GetByIdAsync("profile-1", It.IsAny<CancellationToken>()))
            .ReturnsAsync(profile);
        _accountRepository.Setup(r => r.GetByIdAsync("account-1", It.IsAny<CancellationToken>()))
            .ReturnsAsync(account);

        var result = await RemoveProfileFromAccountCommandHandler.Handle(
            command, _profileRepository.Object, _accountRepository.Object,
            _unitOfWork.Object, _logger.Object, CancellationToken.None);

        result.Should().BeTrue();
        profile.AccountId.Should().BeNull();
        account.UserProfileIds.Should().NotContain("profile-1");
        account.UserProfileIds.Should().Contain("profile-2");
        _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ProfileNotFound_ReturnsFalse()
    {
        var command = new RemoveProfileFromAccountCommand("missing");

        _profileRepository.Setup(r => r.GetByIdAsync("missing", It.IsAny<CancellationToken>()))
            .ReturnsAsync(default(UserProfile));

        var result = await RemoveProfileFromAccountCommandHandler.Handle(
            command, _profileRepository.Object, _accountRepository.Object,
            _unitOfWork.Object, _logger.Object, CancellationToken.None);

        result.Should().BeFalse();
        _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_ProfileNotLinkedToAccount_ReturnsTrue()
    {
        var profile = new UserProfile { Id = "profile-1", AccountId = null! };
        var command = new RemoveProfileFromAccountCommand("profile-1");

        _profileRepository.Setup(r => r.GetByIdAsync("profile-1", It.IsAny<CancellationToken>()))
            .ReturnsAsync(profile);

        var result = await RemoveProfileFromAccountCommandHandler.Handle(
            command, _profileRepository.Object, _accountRepository.Object,
            _unitOfWork.Object, _logger.Object, CancellationToken.None);

        result.Should().BeTrue();
        _accountRepository.Verify(r => r.GetByIdAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_AccountNotFound_StillClearsProfileLink()
    {
        var profile = new UserProfile { Id = "profile-1", AccountId = "account-1" };
        var command = new RemoveProfileFromAccountCommand("profile-1");

        _profileRepository.Setup(r => r.GetByIdAsync("profile-1", It.IsAny<CancellationToken>()))
            .ReturnsAsync(profile);
        _accountRepository.Setup(r => r.GetByIdAsync("account-1", It.IsAny<CancellationToken>()))
            .ReturnsAsync(default(Account));

        var result = await RemoveProfileFromAccountCommandHandler.Handle(
            command, _profileRepository.Object, _accountRepository.Object,
            _unitOfWork.Object, _logger.Object, CancellationToken.None);

        result.Should().BeTrue();
        profile.AccountId.Should().BeNull();
        _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ProfileIdNotInAccountList_StillClearsProfileLink()
    {
        var profile = new UserProfile { Id = "profile-1", AccountId = "account-1" };
        var account = new Account
        {
            Id = "account-1",
            UserProfileIds = new List<string> { "profile-2" } // profile-1 not in list
        };
        var command = new RemoveProfileFromAccountCommand("profile-1");

        _profileRepository.Setup(r => r.GetByIdAsync("profile-1", It.IsAny<CancellationToken>()))
            .ReturnsAsync(profile);
        _accountRepository.Setup(r => r.GetByIdAsync("account-1", It.IsAny<CancellationToken>()))
            .ReturnsAsync(account);

        var result = await RemoveProfileFromAccountCommandHandler.Handle(
            command, _profileRepository.Object, _accountRepository.Object,
            _unitOfWork.Object, _logger.Object, CancellationToken.None);

        result.Should().BeTrue();
        profile.AccountId.Should().BeNull();
        account.UserProfileIds.Should().HaveCount(1); // profile-2 remains
    }

    [Fact]
    public async Task Handle_PropagatesCancellationToken()
    {
        using var cts = new CancellationTokenSource();
        var ct = cts.Token;
        var profile = new UserProfile { Id = "profile-1", AccountId = "account-1" };
        var account = new Account
        {
            Id = "account-1",
            UserProfileIds = new List<string> { "profile-1" }
        };
        var command = new RemoveProfileFromAccountCommand("profile-1");

        _profileRepository.Setup(r => r.GetByIdAsync("profile-1", ct)).ReturnsAsync(profile);
        _accountRepository.Setup(r => r.GetByIdAsync("account-1", ct)).ReturnsAsync(account);

        await RemoveProfileFromAccountCommandHandler.Handle(
            command, _profileRepository.Object, _accountRepository.Object,
            _unitOfWork.Object, _logger.Object, ct);

        _profileRepository.Verify(r => r.GetByIdAsync("profile-1", ct), Times.Once);
        _accountRepository.Verify(r => r.GetByIdAsync("account-1", ct), Times.Once);
        _profileRepository.Verify(r => r.UpdateAsync(profile, ct), Times.Once);
        _accountRepository.Verify(r => r.UpdateAsync(account, ct), Times.Once);
        _unitOfWork.Verify(u => u.SaveChangesAsync(ct), Times.Once);
    }
}
