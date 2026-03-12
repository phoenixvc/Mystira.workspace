using Mystira.Shared.Exceptions;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Mystira.App.Application.CQRS.Accounts.Commands;
using Mystira.Core.Ports.Data;
using Mystira.App.Application.UseCases.Accounts;
using Mystira.Domain.Models;
using Mystira.Domain.Enums;
using Mystira.Domain.ValueObjects;
using Mystira.Shared.Data.Repositories;

namespace Mystira.App.Application.Tests.CQRS.Accounts;

public class AddCompletedScenarioCommandHandlerTests
{
    private readonly Mock<IAccountRepository> _repository;
    private readonly Mock<IUnitOfWork> _unitOfWork;
    private readonly Mock<ILogger<AddCompletedScenarioUseCase>> _useCaseLogger;
    private readonly Mock<ILogger> _handlerLogger;

    public AddCompletedScenarioCommandHandlerTests()
    {
        _repository = new Mock<IAccountRepository>();
        _unitOfWork = new Mock<IUnitOfWork>();
        _useCaseLogger = new Mock<ILogger<AddCompletedScenarioUseCase>>();
        _handlerLogger = new Mock<ILogger>();
    }

    [Fact]
    public async Task Handle_WithNewScenario_AddsToCompletedList()
    {
        var account = new Account
        {
            Id = "acc-1",
            CompletedScenarioIds = new List<string> { "old-scenario" }
        };
        var command = new AddCompletedScenarioCommand("acc-1", "new-scenario");

        _repository.Setup(r => r.GetByIdAsync("acc-1", It.IsAny<CancellationToken>()))
            .ReturnsAsync(account);

        var useCase = new AddCompletedScenarioUseCase(_repository.Object, _unitOfWork.Object, _useCaseLogger.Object);

        var result = await AddCompletedScenarioCommandHandler.Handle(
            command, useCase, _handlerLogger.Object, CancellationToken.None);

        result.Should().BeTrue();
        account.CompletedScenarioIds.Should().Contain("new-scenario");
        _repository.Verify(r => r.UpdateAsync(account, It.IsAny<CancellationToken>()), Times.Once);
        _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WithAlreadyCompletedScenario_DoesNotUpdate()
    {
        var account = new Account
        {
            Id = "acc-1",
            CompletedScenarioIds = new List<string> { "existing" }
        };
        var command = new AddCompletedScenarioCommand("acc-1", "existing");

        _repository.Setup(r => r.GetByIdAsync("acc-1", It.IsAny<CancellationToken>()))
            .ReturnsAsync(account);

        var useCase = new AddCompletedScenarioUseCase(_repository.Object, _unitOfWork.Object, _useCaseLogger.Object);

        var result = await AddCompletedScenarioCommandHandler.Handle(
            command, useCase, _handlerLogger.Object, CancellationToken.None);

        result.Should().BeTrue();
        _repository.Verify(r => r.UpdateAsync(It.IsAny<Account>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_AccountNotFound_ThrowsNotFoundException()
    {
        var command = new AddCompletedScenarioCommand("missing", "scenario-1");
        _repository.Setup(r => r.GetByIdAsync("missing", It.IsAny<CancellationToken>()))
            .ReturnsAsync(default(Account));

        var useCase = new AddCompletedScenarioUseCase(_repository.Object, _unitOfWork.Object, _useCaseLogger.Object);

        var act = () => AddCompletedScenarioCommandHandler.Handle(
            command, useCase, _handlerLogger.Object, CancellationToken.None);

        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Theory]
    [InlineData(null, "scenario-1")]
    [InlineData("", "scenario-1")]
    [InlineData("acc-1", null)]
    [InlineData("acc-1", "")]
    public async Task Handle_WithNullOrEmptyIds_ThrowsValidationException(string? accountId, string? scenarioId)
    {
        var command = new AddCompletedScenarioCommand(accountId!, scenarioId!);

        var useCase = new AddCompletedScenarioUseCase(_repository.Object, _unitOfWork.Object, _useCaseLogger.Object);

        var act = () => AddCompletedScenarioCommandHandler.Handle(
            command, useCase, _handlerLogger.Object, CancellationToken.None);

        await act.Should().ThrowAsync<ValidationException>();
    }

    [Fact]
    public async Task Handle_WithNullCompletedList_InitializesAndAdds()
    {
        var account = new Account { Id = "acc-1", CompletedScenarioIds = null! };
        var command = new AddCompletedScenarioCommand("acc-1", "first-scenario");

        _repository.Setup(r => r.GetByIdAsync("acc-1", It.IsAny<CancellationToken>()))
            .ReturnsAsync(account);

        var useCase = new AddCompletedScenarioUseCase(_repository.Object, _unitOfWork.Object, _useCaseLogger.Object);

        var result = await AddCompletedScenarioCommandHandler.Handle(
            command, useCase, _handlerLogger.Object, CancellationToken.None);

        result.Should().BeTrue();
        account.CompletedScenarioIds.Should().Contain("first-scenario");
    }

    [Fact]
    public async Task Handle_PropagatesCancellationToken()
    {
        using var cts = new CancellationTokenSource();
        var ct = cts.Token;
        var account = new Account
        {
            Id = "acc-1",
            CompletedScenarioIds = new List<string>()
        };
        var command = new AddCompletedScenarioCommand("acc-1", "scenario-1");

        _repository.Setup(r => r.GetByIdAsync("acc-1", ct)).ReturnsAsync(account);

        var useCase = new AddCompletedScenarioUseCase(_repository.Object, _unitOfWork.Object, _useCaseLogger.Object);

        await AddCompletedScenarioCommandHandler.Handle(
            command, useCase, _handlerLogger.Object, ct);

        _repository.Verify(r => r.GetByIdAsync("acc-1", ct), Times.Once);
        _repository.Verify(r => r.UpdateAsync(account, ct), Times.Once);
        _unitOfWork.Verify(u => u.SaveChangesAsync(ct), Times.Once);
    }
}
