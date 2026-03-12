using Mystira.Shared.Exceptions;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Mystira.Core.Ports.Data;
using Mystira.Core.UseCases.Accounts;
using Mystira.Domain.Models;
using Mystira.Domain.Enums;
using Mystira.Domain.ValueObjects;
using Mystira.Shared.Data.Repositories;

namespace Mystira.App.Application.Tests.UseCases.Accounts;

public class UpdateAccountSettingsUseCaseTests
{
    private readonly Mock<IAccountRepository> _repository;
    private readonly Mock<IUnitOfWork> _unitOfWork;
    private readonly Mock<ILogger<UpdateAccountSettingsUseCase>> _logger;
    private readonly UpdateAccountSettingsUseCase _useCase;

    public UpdateAccountSettingsUseCaseTests()
    {
        _repository = new Mock<IAccountRepository>();
        _unitOfWork = new Mock<IUnitOfWork>();
        _logger = new Mock<ILogger<UpdateAccountSettingsUseCase>>();
        _useCase = new UpdateAccountSettingsUseCase(_repository.Object, _unitOfWork.Object, _logger.Object);
    }

    [Fact]
    public async Task ExecuteAsync_WithValidSettings_ReplacesAccountSettings()
    {
        var account = new Account
        {
            Id = "acc-1",
            Settings = new AccountSettings { PreferredLanguage = "en", Theme = "Light" }
        };
        _repository.Setup(r => r.GetByIdAsync("acc-1", It.IsAny<CancellationToken>()))
            .ReturnsAsync(account);

        var newSettings = new AccountSettings
        {
            PreferredLanguage = "de",
            NotificationsEnabled = false,
            Theme = "Dark"
        };

        var result = await _useCase.ExecuteAsync("acc-1", newSettings);

        result.Settings.Should().BeSameAs(newSettings);
        _repository.Verify(r => r.UpdateAsync(account, It.IsAny<CancellationToken>()), Times.Once);
        _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ExecuteAsync_WithNonExistingAccount_ThrowsValidationException()
    {
        _repository.Setup(r => r.GetByIdAsync("missing", It.IsAny<CancellationToken>()))
            .ReturnsAsync(default(Account));

        var act = () => _useCase.ExecuteAsync("missing", new AccountSettings());

        await act.Should().ThrowAsync<NotFoundException>().WithMessage("*not found*");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public async Task ExecuteAsync_WithNullOrEmptyId_ThrowsValidationException(string? accountId)
    {
        var act = () => _useCase.ExecuteAsync(accountId!, new AccountSettings());

        await act.Should().ThrowAsync<ValidationException>();
    }

    [Fact]
    public async Task ExecuteAsync_WithNullSettings_ThrowsValidationException()
    {
        var act = () => _useCase.ExecuteAsync("acc-1", null!);

        await act.Should().ThrowAsync<ValidationException>();
    }
}
