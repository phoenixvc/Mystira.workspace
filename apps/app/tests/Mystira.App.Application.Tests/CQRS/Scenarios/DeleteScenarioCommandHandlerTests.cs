using Mystira.Shared.Exceptions;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Mystira.Core.CQRS.Scenarios.Commands;
using Mystira.Core.Ports.Data;
using Mystira.Domain.Models;
using Mystira.Domain.Enums;
using Mystira.Domain.ValueObjects;
using Mystira.Shared.Data.Repositories;

namespace Mystira.App.Application.Tests.CQRS.Scenarios;

public class DeleteScenarioCommandHandlerTests
{
    private readonly Mock<IScenarioRepository> _repository;
    private readonly Mock<IUnitOfWork> _unitOfWork;
    private readonly Mock<ILogger> _logger;

    public DeleteScenarioCommandHandlerTests()
    {
        _repository = new Mock<IScenarioRepository>();
        _unitOfWork = new Mock<IUnitOfWork>();
        _logger = new Mock<ILogger>();
    }

    [Fact]
    public async Task Handle_WithExistingScenario_DeletesSuccessfully()
    {
        var scenario = new Scenario { Id = "scen-1", Title = "Test Scenario" };
        _repository.Setup(r => r.GetByIdAsync("scen-1", It.IsAny<CancellationToken>())).ReturnsAsync(scenario);
        _repository.Setup(r => r.DeleteAsync("scen-1", It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
        _unitOfWork.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        await DeleteScenarioCommandHandler.Handle(
            new DeleteScenarioCommand("scen-1"),
            _repository.Object, _unitOfWork.Object, _logger.Object, CancellationToken.None);

        _repository.Verify(r => r.DeleteAsync("scen-1", It.IsAny<CancellationToken>()), Times.Once);
        _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WithNonExistingScenario_ThrowsInvalidOperationException()
    {
        _repository.Setup(r => r.GetByIdAsync("missing", It.IsAny<CancellationToken>())).ReturnsAsync(default(Scenario));

        var act = () => DeleteScenarioCommandHandler.Handle(
            new DeleteScenarioCommand("missing"),
            _repository.Object, _unitOfWork.Object, _logger.Object, CancellationToken.None);

        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task Handle_WithEmptyScenarioId_ThrowsValidationException()
    {
        var act = () => DeleteScenarioCommandHandler.Handle(
            new DeleteScenarioCommand(""),
            _repository.Object, _unitOfWork.Object, _logger.Object, CancellationToken.None);

        await act.Should().ThrowAsync<ValidationException>();
    }
}
