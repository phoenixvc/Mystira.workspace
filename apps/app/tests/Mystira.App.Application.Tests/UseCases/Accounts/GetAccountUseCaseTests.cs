using Mystira.Shared.Exceptions;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Mystira.Core.Ports.Data;
using Mystira.App.Application.UseCases.Accounts;
using Mystira.Domain.Models;
using Mystira.Domain.Enums;
using Mystira.Domain.ValueObjects;

namespace Mystira.App.Application.Tests.UseCases.Accounts;

public class GetAccountUseCaseTests
{
    private readonly Mock<IAccountRepository> _repository;
    private readonly Mock<ILogger<GetAccountUseCase>> _logger;
    private readonly GetAccountUseCase _useCase;

    public GetAccountUseCaseTests()
    {
        _repository = new Mock<IAccountRepository>();
        _logger = new Mock<ILogger<GetAccountUseCase>>();
        _useCase = new GetAccountUseCase(_repository.Object, _logger.Object);
    }

    [Fact]
    public async Task ExecuteAsync_WithExistingId_ReturnsAccount()
    {
        var account = new Account { Id = "acc-1", Email = "test@example.com", DisplayName = "Test" };
        _repository.Setup(r => r.GetByIdAsync("acc-1", It.IsAny<CancellationToken>()))
            .ReturnsAsync(account);

        var result = await _useCase.ExecuteAsync("acc-1");

        result.Should().NotBeNull();
        result!.Id.Should().Be("acc-1");
        result.Email.Should().Be("test@example.com");
    }

    [Fact]
    public async Task ExecuteAsync_WithNonExistingId_ReturnsNull()
    {
        _repository.Setup(r => r.GetByIdAsync("missing", It.IsAny<CancellationToken>()))
            .ReturnsAsync(default(Account));

        var result = await _useCase.ExecuteAsync("missing");

        result.Should().BeNull();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public async Task ExecuteAsync_WithNullOrEmptyId_ThrowsValidationException(string? accountId)
    {
        var act = () => _useCase.ExecuteAsync(accountId!);

        await act.Should().ThrowAsync<ValidationException>();
    }

    [Fact]
    public async Task ExecuteAsync_CallsRepositoryOnce()
    {
        _repository.Setup(r => r.GetByIdAsync("acc-1", It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Account { Id = "acc-1" });

        await _useCase.ExecuteAsync("acc-1");

        _repository.Verify(r => r.GetByIdAsync("acc-1", It.IsAny<CancellationToken>()), Times.Once);
    }
}
