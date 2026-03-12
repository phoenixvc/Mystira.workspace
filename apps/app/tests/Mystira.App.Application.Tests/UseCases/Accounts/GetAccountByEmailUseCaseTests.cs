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

public class GetAccountByEmailUseCaseTests
{
    private readonly Mock<IAccountRepository> _repository;
    private readonly Mock<ILogger<GetAccountByEmailUseCase>> _logger;
    private readonly GetAccountByEmailUseCase _useCase;

    public GetAccountByEmailUseCaseTests()
    {
        _repository = new Mock<IAccountRepository>();
        _logger = new Mock<ILogger<GetAccountByEmailUseCase>>();
        _useCase = new GetAccountByEmailUseCase(_repository.Object, _logger.Object);
    }

    [Fact]
    public async Task ExecuteAsync_WithExistingEmail_ReturnsAccount()
    {
        var account = new Account { Id = "acc-1", Email = "test@example.com" };
        _repository.Setup(r => r.GetByEmailAsync("test@example.com", It.IsAny<CancellationToken>()))
            .ReturnsAsync(account);

        var result = await _useCase.ExecuteAsync("test@example.com");

        result.Should().NotBeNull();
        result!.Email.Should().Be("test@example.com");
    }

    [Fact]
    public async Task ExecuteAsync_WithNonExistingEmail_ReturnsNull()
    {
        _repository.Setup(r => r.GetByEmailAsync("unknown@example.com", It.IsAny<CancellationToken>()))
            .ReturnsAsync(default(Account));

        var result = await _useCase.ExecuteAsync("unknown@example.com");

        result.Should().BeNull();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public async Task ExecuteAsync_WithNullOrEmptyEmail_ThrowsValidationException(string? email)
    {
        var act = () => _useCase.ExecuteAsync(email!);

        await act.Should().ThrowAsync<ValidationException>();
    }
}
