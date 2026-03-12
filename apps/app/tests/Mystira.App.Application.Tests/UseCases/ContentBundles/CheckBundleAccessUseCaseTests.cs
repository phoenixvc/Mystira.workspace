using Mystira.Shared.Exceptions;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Mystira.Core.Ports.Data;
using Mystira.App.Application.UseCases.ContentBundles;
using Mystira.Domain.Models;
using Mystira.Domain.Enums;
using Mystira.Domain.ValueObjects;

namespace Mystira.App.Application.Tests.UseCases.ContentBundles;

public class CheckBundleAccessUseCaseTests
{
    private readonly Mock<IContentBundleRepository> _bundleRepository;
    private readonly Mock<IAccountRepository> _accountRepository;
    private readonly Mock<ILogger<CheckBundleAccessUseCase>> _logger;
    private readonly CheckBundleAccessUseCase _useCase;

    public CheckBundleAccessUseCaseTests()
    {
        _bundleRepository = new Mock<IContentBundleRepository>();
        _accountRepository = new Mock<IAccountRepository>();
        _logger = new Mock<ILogger<CheckBundleAccessUseCase>>();
        _useCase = new CheckBundleAccessUseCase(
            _bundleRepository.Object, _accountRepository.Object, _logger.Object);
    }

    [Fact]
    public async Task ExecuteAsync_WithFreeBundle_ReturnsTrue()
    {
        var bundle = new ContentBundle { Id = "b1", PriceCents = 0 };
        _bundleRepository.Setup(r => r.GetByIdAsync("b1", It.IsAny<CancellationToken>()))
            .ReturnsAsync(bundle);

        var result = await _useCase.ExecuteAsync("acc-1", "b1");

        result.Should().BeTrue();
    }

    [Fact]
    public async Task ExecuteAsync_WithPaidBundleAndActiveSubscription_ReturnsTrue()
    {
        var bundle = new ContentBundle { Id = "b1", PriceCents = 999 };
        var account = new Account
        {
            Id = "acc-1",
            Subscription = new SubscriptionDetails
            {
                Type = SubscriptionType.Monthly,
                IsActive = true,
                ValidUntil = DateTime.UtcNow.AddMonths(1)
            }
        };
        _bundleRepository.Setup(r => r.GetByIdAsync("b1", It.IsAny<CancellationToken>()))
            .ReturnsAsync(bundle);
        _accountRepository.Setup(r => r.GetByIdAsync("acc-1", It.IsAny<CancellationToken>()))
            .ReturnsAsync(account);

        var result = await _useCase.ExecuteAsync("acc-1", "b1");

        result.Should().BeTrue();
    }

    [Fact]
    public async Task ExecuteAsync_WithNonExistingBundle_ReturnsFalse()
    {
        _bundleRepository.Setup(r => r.GetByIdAsync("missing", It.IsAny<CancellationToken>()))
            .ReturnsAsync(default(ContentBundle));

        var result = await _useCase.ExecuteAsync("acc-1", "missing");

        result.Should().BeFalse();
    }

    [Theory]
    [InlineData(null, "b1")]
    [InlineData("", "b1")]
    [InlineData("acc-1", null)]
    [InlineData("acc-1", "")]
    public async Task ExecuteAsync_WithNullOrEmptyIds_ThrowsValidationException(string? accountId, string? bundleId)
    {
        var act = () => _useCase.ExecuteAsync(accountId!, bundleId!);

        await act.Should().ThrowAsync<ValidationException>();
    }
}
