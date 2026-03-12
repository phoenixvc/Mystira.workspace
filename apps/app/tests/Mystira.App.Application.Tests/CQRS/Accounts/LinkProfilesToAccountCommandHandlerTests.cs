using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Mystira.App.Application.CQRS.Accounts.Commands;
using Mystira.Core.Ports.Data;
using Mystira.Domain.Models;
using Mystira.Domain.Enums;
using Mystira.Domain.ValueObjects;
using Mystira.Shared.Data.Repositories;

namespace Mystira.App.Application.Tests.CQRS.Accounts;

public class LinkProfilesToAccountCommandHandlerTests
{
    private readonly Mock<IAccountRepository> _accountRepository;
    private readonly Mock<IUserProfileRepository> _profileRepository;
    private readonly Mock<IUnitOfWork> _unitOfWork;
    private readonly Mock<ILogger> _logger;

    public LinkProfilesToAccountCommandHandlerTests()
    {
        _accountRepository = new Mock<IAccountRepository>();
        _profileRepository = new Mock<IUserProfileRepository>();
        _unitOfWork = new Mock<IUnitOfWork>();
        _logger = new Mock<ILogger>();
    }

    [Fact]
    public async Task Handle_LinksProfileToAccount()
    {
        var account = new Account { Id = "acc-1", UserProfileIds = new List<string>() };
        var profile = new UserProfile { Id = "profile-1", AccountId = null! };
        var command = new LinkProfilesToAccountCommand("acc-1", new List<string> { "profile-1" });

        _accountRepository.Setup(r => r.GetByIdAsync("acc-1", It.IsAny<CancellationToken>()))
            .ReturnsAsync(account);
        _profileRepository.Setup(r => r.GetByIdAsync("profile-1", It.IsAny<CancellationToken>()))
            .ReturnsAsync(profile);

        var result = await LinkProfilesToAccountCommandHandler.Handle(
            command, _accountRepository.Object, _profileRepository.Object,
            _unitOfWork.Object, _logger.Object, CancellationToken.None);

        result.Should().BeTrue();
        profile.AccountId.Should().Be("acc-1");
        account.UserProfileIds.Should().Contain("profile-1");
        _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_AccountNotFound_ReturnsFalse()
    {
        var command = new LinkProfilesToAccountCommand("missing", new List<string> { "profile-1" });
        _accountRepository.Setup(r => r.GetByIdAsync("missing", It.IsAny<CancellationToken>()))
            .ReturnsAsync(default(Account));

        var result = await LinkProfilesToAccountCommandHandler.Handle(
            command, _accountRepository.Object, _profileRepository.Object,
            _unitOfWork.Object, _logger.Object, CancellationToken.None);

        result.Should().BeFalse();
    }

    [Fact]
    public async Task Handle_ProfileAlreadyLinked_SkipsProfile()
    {
        var account = new Account { Id = "acc-1", UserProfileIds = new List<string> { "profile-1" } };
        var profile = new UserProfile { Id = "profile-1", AccountId = "acc-1" };
        var command = new LinkProfilesToAccountCommand("acc-1", new List<string> { "profile-1" });

        _accountRepository.Setup(r => r.GetByIdAsync("acc-1", It.IsAny<CancellationToken>()))
            .ReturnsAsync(account);
        _profileRepository.Setup(r => r.GetByIdAsync("profile-1", It.IsAny<CancellationToken>()))
            .ReturnsAsync(profile);

        var result = await LinkProfilesToAccountCommandHandler.Handle(
            command, _accountRepository.Object, _profileRepository.Object,
            _unitOfWork.Object, _logger.Object, CancellationToken.None);

        result.Should().BeFalse();
        _profileRepository.Verify(r => r.UpdateAsync(It.IsAny<UserProfile>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_MissingProfile_SkipsAndContinues()
    {
        var account = new Account { Id = "acc-1", UserProfileIds = new List<string>() };
        var profile2 = new UserProfile { Id = "profile-2", AccountId = null! };
        var command = new LinkProfilesToAccountCommand("acc-1", new List<string> { "missing", "profile-2" });

        _accountRepository.Setup(r => r.GetByIdAsync("acc-1", It.IsAny<CancellationToken>()))
            .ReturnsAsync(account);
        _profileRepository.Setup(r => r.GetByIdAsync("missing", It.IsAny<CancellationToken>()))
            .ReturnsAsync(default(UserProfile));
        _profileRepository.Setup(r => r.GetByIdAsync("profile-2", It.IsAny<CancellationToken>()))
            .ReturnsAsync(profile2);

        var result = await LinkProfilesToAccountCommandHandler.Handle(
            command, _accountRepository.Object, _profileRepository.Object,
            _unitOfWork.Object, _logger.Object, CancellationToken.None);

        result.Should().BeTrue();
        account.UserProfileIds.Should().Contain("profile-2");
        account.UserProfileIds.Should().NotContain("missing");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public async Task Handle_NullOrEmptyAccountId_ReturnsFalse(string? accountId)
    {
        var command = new LinkProfilesToAccountCommand(accountId!, new List<string> { "profile-1" });

        var result = await LinkProfilesToAccountCommandHandler.Handle(
            command, _accountRepository.Object, _profileRepository.Object,
            _unitOfWork.Object, _logger.Object, CancellationToken.None);

        result.Should().BeFalse();
    }

    [Fact]
    public async Task Handle_NullProfileIdsList_ReturnsFalse()
    {
        var command = new LinkProfilesToAccountCommand("acc-1", null!);

        var result = await LinkProfilesToAccountCommandHandler.Handle(
            command, _accountRepository.Object, _profileRepository.Object,
            _unitOfWork.Object, _logger.Object, CancellationToken.None);

        result.Should().BeFalse();
    }
}
