using Mystira.Shared.Exceptions;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Mystira.Core.Ports.Data;
using Mystira.App.Application.UseCases.GameSessions;
using Mystira.Domain.Models;
using Mystira.Domain.Enums;
using Mystira.Domain.ValueObjects;

namespace Mystira.App.Application.Tests.UseCases;

public class SelectCharacterUseCaseTests
{
    private readonly Mock<IGameSessionRepository> _repository;
    private readonly Mock<IUnitOfWork> _unitOfWork;
    private readonly Mock<ILogger<SelectCharacterUseCase>> _logger;
    private readonly SelectCharacterUseCase _useCase;

    public SelectCharacterUseCaseTests()
    {
        _repository = new Mock<IGameSessionRepository>();
        _unitOfWork = new Mock<IUnitOfWork>();
        _logger = new Mock<ILogger<SelectCharacterUseCase>>();
        _useCase = new SelectCharacterUseCase(
            _repository.Object, _unitOfWork.Object, _logger.Object);
    }

    [Fact]
    public async Task ExecuteAsync_WithValidInput_SetsSelectedCharacter()
    {
        var session = new GameSession { Id = "session-1", ScenarioId = "s1" };
        _repository.Setup(r => r.GetByIdAsync("session-1", It.IsAny<CancellationToken>()))
            .ReturnsAsync(session);

        var result = await _useCase.ExecuteAsync("session-1", "char-1");

        result.Should().NotBeNull();
        result.SelectedCharacterId.Should().Be("char-1");
        _repository.Verify(r => r.UpdateAsync(session, It.IsAny<CancellationToken>()), Times.Once);
        _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ExecuteAsync_WithNonExistentSession_ThrowsValidationException()
    {
        _repository.Setup(r => r.GetByIdAsync("missing", It.IsAny<CancellationToken>()))
            .ReturnsAsync(default(GameSession));

        var act = () => _useCase.ExecuteAsync("missing", "char-1");

        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public async Task ExecuteAsync_WithNullOrEmptySessionId_ThrowsValidationException(string? sessionId)
    {
        var act = () => _useCase.ExecuteAsync(sessionId!, "char-1");

        await act.Should().ThrowAsync<ValidationException>();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public async Task ExecuteAsync_WithNullOrEmptyCharacterId_ThrowsValidationException(string? characterId)
    {
        var act = () => _useCase.ExecuteAsync("session-1", characterId!);

        await act.Should().ThrowAsync<ValidationException>();
    }

    [Fact]
    public async Task ExecuteAsync_CanChangeCharacterSelection()
    {
        var session = new GameSession
        {
            Id = "session-1",
            ScenarioId = "s1",
            SelectedCharacterId = "char-old"
        };
        _repository.Setup(r => r.GetByIdAsync("session-1", It.IsAny<CancellationToken>()))
            .ReturnsAsync(session);

        var result = await _useCase.ExecuteAsync("session-1", "char-new");

        result.SelectedCharacterId.Should().Be("char-new");
    }
}
