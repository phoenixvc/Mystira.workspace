using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Mystira.App.Application.Ports.Data;
using Mystira.App.Application.UseCases.Accounts;
using Mystira.App.Domain.Models;
using Mystira.Contracts.App.Requests.Accounts;
using ContractSubscriptionType = Mystira.Contracts.App.Enums.SubscriptionType;
using Mystira.Shared.Data.Repositories;

namespace Mystira.App.Application.Tests.UseCases.Accounts;

public class UpdateSubscriptionUseCaseTests
{
    private readonly Mock<IAccountRepository> _repository;
    private readonly Mock<IUnitOfWork> _unitOfWork;
    private readonly Mock<ILogger<UpdateSubscriptionUseCase>> _logger;
    private readonly UpdateSubscriptionUseCase _useCase;

    public UpdateSubscriptionUseCaseTests()
    {
        _repository = new Mock<IAccountRepository>();
        _unitOfWork = new Mock<IUnitOfWork>();
        _logger = new Mock<ILogger<UpdateSubscriptionUseCase>>();
        _useCase = new UpdateSubscriptionUseCase(_repository.Object, _unitOfWork.Object, _logger.Object);
    }

    [Fact]
    public async Task ExecuteAsync_WithValidRequest_UpdatesSubscriptionType()
    {
        var account = new Account { Id = "acc-1", Subscription = new SubscriptionDetails() };
        _repository.Setup(r => r.GetByIdAsync("acc-1", It.IsAny<CancellationToken>()))
            .ReturnsAsync(account);

        var request = new UpdateSubscriptionRequest
        {
            Type = (ContractSubscriptionType)1 // Maps to domain SubscriptionType.Monthly via int cast
        };

        var result = await _useCase.ExecuteAsync("acc-1", request);

        result.Subscription.Type.Should().Be(Mystira.App.Domain.Models.SubscriptionType.Monthly);
        _repository.Verify(r => r.UpdateAsync(account, It.IsAny<CancellationToken>()), Times.Once);
        _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ExecuteAsync_WithOptionalFields_UpdatesOnlyProvidedFields()
    {
        var account = new Account
        {
            Id = "acc-1",
            Subscription = new SubscriptionDetails
            {
                ProductId = "original-product",
                PurchaseToken = "original-token"
            }
        };
        _repository.Setup(r => r.GetByIdAsync("acc-1", It.IsAny<CancellationToken>()))
            .ReturnsAsync(account);

        var request = new UpdateSubscriptionRequest
        {
            Type = ContractSubscriptionType.Free,
            ProductId = "new-product",
            ValidUntil = new DateTime(2027, 1, 1, 0, 0, 0, DateTimeKind.Utc)
        };

        var result = await _useCase.ExecuteAsync("acc-1", request);

        result.Subscription.ProductId.Should().Be("new-product");
        result.Subscription.ValidUntil.Should().Be(new DateTime(2027, 1, 1, 0, 0, 0, DateTimeKind.Utc));
        result.Subscription.PurchaseToken.Should().Be("original-token");
    }

    [Fact]
    public async Task ExecuteAsync_SetsLastVerifiedTimestamp()
    {
        var account = new Account { Id = "acc-1", Subscription = new SubscriptionDetails() };
        _repository.Setup(r => r.GetByIdAsync("acc-1", It.IsAny<CancellationToken>()))
            .ReturnsAsync(account);

        var before = DateTime.UtcNow;
        var request = new UpdateSubscriptionRequest { Type = ContractSubscriptionType.Free };

        var result = await _useCase.ExecuteAsync("acc-1", request);

        result.Subscription.LastVerified.Should().BeOnOrAfter(before);
    }

    [Fact]
    public async Task ExecuteAsync_WithNonExistingAccount_ThrowsArgumentException()
    {
        _repository.Setup(r => r.GetByIdAsync("missing", It.IsAny<CancellationToken>()))
            .ReturnsAsync(default(Account));

        var act = () => _useCase.ExecuteAsync("missing", new UpdateSubscriptionRequest { Type = ContractSubscriptionType.Free });

        await act.Should().ThrowAsync<ArgumentException>().WithMessage("*not found*");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public async Task ExecuteAsync_WithNullOrEmptyId_ThrowsArgumentException(string? accountId)
    {
        var act = () => _useCase.ExecuteAsync(accountId!, new UpdateSubscriptionRequest { Type = ContractSubscriptionType.Free });

        await act.Should().ThrowAsync<ArgumentException>();
    }

    [Fact]
    public async Task ExecuteAsync_WithNullRequest_ThrowsArgumentNullException()
    {
        var act = () => _useCase.ExecuteAsync("acc-1", null!);

        await act.Should().ThrowAsync<ArgumentNullException>();
    }
}
