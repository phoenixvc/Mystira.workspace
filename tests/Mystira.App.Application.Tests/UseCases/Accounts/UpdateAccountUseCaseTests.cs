using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Mystira.App.Application.Ports.Data;
using Mystira.App.Application.UseCases.Accounts;
using Mystira.App.Domain.Models;
using Mystira.Contracts.App.Requests.Accounts;
using Mystira.Shared.Data.Repositories;

namespace Mystira.App.Application.Tests.UseCases.Accounts;

public class UpdateAccountUseCaseTests
{
    private readonly Mock<IAccountRepository> _repository;
    private readonly Mock<IUnitOfWork> _unitOfWork;
    private readonly Mock<ILogger<UpdateAccountUseCase>> _logger;
    private readonly UpdateAccountUseCase _useCase;

    public UpdateAccountUseCaseTests()
    {
        _repository = new Mock<IAccountRepository>();
        _unitOfWork = new Mock<IUnitOfWork>();
        _logger = new Mock<ILogger<UpdateAccountUseCase>>();
        _useCase = new UpdateAccountUseCase(_repository.Object, _unitOfWork.Object, _logger.Object);
    }

    [Fact]
    public async Task ExecuteAsync_WithValidRequest_UpdatesDisplayName()
    {
        var account = new Account { Id = "acc-1", DisplayName = "Old Name" };
        _repository.Setup(r => r.GetByIdAsync("acc-1", It.IsAny<CancellationToken>()))
            .ReturnsAsync(account);

        var request = new UpdateAccountRequest { DisplayName = "New Name" };

        var result = await _useCase.ExecuteAsync("acc-1", request);

        result.DisplayName.Should().Be("New Name");
        _repository.Verify(r => r.UpdateAsync(account, It.IsAny<CancellationToken>()), Times.Once);
        _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ExecuteAsync_WithSettings_UpdatesAccountSettings()
    {
        var account = new Account { Id = "acc-1", Settings = new AccountSettings() };
        _repository.Setup(r => r.GetByIdAsync("acc-1", It.IsAny<CancellationToken>()))
            .ReturnsAsync(account);

        var request = new UpdateAccountRequest
        {
            Settings = new AccountSettings
            {
                PreferredLanguage = "fr",
                NotificationsEnabled = false,
                Theme = "Dark"
            }
        };

        var result = await _useCase.ExecuteAsync("acc-1", request);

        result.Settings.PreferredLanguage.Should().Be("fr");
        result.Settings.NotificationsEnabled.Should().BeFalse();
        result.Settings.Theme.Should().Be("Dark");
    }

    [Fact]
    public async Task ExecuteAsync_UpdatesLastLoginAt()
    {
        var account = new Account { Id = "acc-1", LastLoginAt = DateTime.UtcNow.AddDays(-7) };
        _repository.Setup(r => r.GetByIdAsync("acc-1", It.IsAny<CancellationToken>()))
            .ReturnsAsync(account);

        var before = DateTime.UtcNow;
        var result = await _useCase.ExecuteAsync("acc-1", new UpdateAccountRequest());

        result.LastLoginAt.Should().BeOnOrAfter(before);
    }

    [Fact]
    public async Task ExecuteAsync_WithNonExistingAccount_ThrowsArgumentException()
    {
        _repository.Setup(r => r.GetByIdAsync("missing", It.IsAny<CancellationToken>()))
            .ReturnsAsync(default(Account));

        var act = () => _useCase.ExecuteAsync("missing", new UpdateAccountRequest());

        await act.Should().ThrowAsync<ArgumentException>().WithMessage("*not found*");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public async Task ExecuteAsync_WithNullOrEmptyId_ThrowsArgumentException(string? accountId)
    {
        var act = () => _useCase.ExecuteAsync(accountId!, new UpdateAccountRequest());

        await act.Should().ThrowAsync<ArgumentException>();
    }

    [Fact]
    public async Task ExecuteAsync_WithNullRequest_ThrowsArgumentNullException()
    {
        var act = () => _useCase.ExecuteAsync("acc-1", null!);

        await act.Should().ThrowAsync<ArgumentNullException>();
    }
}
