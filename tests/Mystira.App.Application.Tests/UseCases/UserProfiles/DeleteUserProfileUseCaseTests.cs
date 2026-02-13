using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Mystira.App.Application.Ports.Data;
using Mystira.App.Application.UseCases.UserProfiles;
using Mystira.App.Domain.Models;

namespace Mystira.App.Application.Tests.UseCases.UserProfiles;

public class DeleteUserProfileUseCaseTests
{
    private readonly Mock<IUserProfileRepository> _repository;
    private readonly Mock<IGameSessionRepository> _gameSessionRepository;
    private readonly Mock<IUnitOfWork> _unitOfWork;
    private readonly Mock<ILogger<DeleteUserProfileUseCase>> _logger;
    private readonly DeleteUserProfileUseCase _useCase;

    public DeleteUserProfileUseCaseTests()
    {
        _repository = new Mock<IUserProfileRepository>();
        _gameSessionRepository = new Mock<IGameSessionRepository>();
        _unitOfWork = new Mock<IUnitOfWork>();
        _logger = new Mock<ILogger<DeleteUserProfileUseCase>>();
        _useCase = new DeleteUserProfileUseCase(
            _repository.Object, _gameSessionRepository.Object,
            _unitOfWork.Object, _logger.Object);
    }

    [Fact]
    public async Task ExecuteAsync_WithExistingProfile_ReturnsTrue()
    {
        var profile = new UserProfile { Id = "p1", Name = "Player" };
        _repository.Setup(r => r.GetByIdAsync("p1", It.IsAny<CancellationToken>()))
            .ReturnsAsync(profile);
        _gameSessionRepository.Setup(r => r.GetByProfileIdAsync("p1", It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<GameSession>());

        var result = await _useCase.ExecuteAsync("p1");

        result.Should().BeTrue();
        _repository.Verify(r => r.DeleteAsync("p1", It.IsAny<CancellationToken>()), Times.Once);
        _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ExecuteAsync_WithNonExistingProfile_ReturnsFalse()
    {
        _repository.Setup(r => r.GetByIdAsync("missing", It.IsAny<CancellationToken>()))
            .ReturnsAsync(default(UserProfile));

        var result = await _useCase.ExecuteAsync("missing");

        result.Should().BeFalse();
        _repository.Verify(r => r.DeleteAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task ExecuteAsync_WithActiveSessions_DeletesSessionsAndProfile()
    {
        var profile = new UserProfile { Id = "p1", Name = "Player" };
        var sessions = new List<GameSession>
        {
            new() { Id = "s1", AccountId = "a1", ScenarioId = "sc1", Status = SessionStatus.InProgress },
            new() { Id = "s2", AccountId = "a1", ScenarioId = "sc2", Status = SessionStatus.Completed }
        };
        _repository.Setup(r => r.GetByIdAsync("p1", It.IsAny<CancellationToken>()))
            .ReturnsAsync(profile);
        _gameSessionRepository.Setup(r => r.GetByProfileIdAsync("p1", It.IsAny<CancellationToken>()))
            .ReturnsAsync(sessions);

        var result = await _useCase.ExecuteAsync("p1");

        result.Should().BeTrue();
        _gameSessionRepository.Verify(r => r.DeleteAsync("s1", It.IsAny<CancellationToken>()), Times.Once);
        _gameSessionRepository.Verify(r => r.DeleteAsync("s2", It.IsAny<CancellationToken>()), Times.Once);
        _repository.Verify(r => r.DeleteAsync("p1", It.IsAny<CancellationToken>()), Times.Once);
        _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public async Task ExecuteAsync_WithNullOrEmptyId_ThrowsArgumentException(string? id)
    {
        var act = () => _useCase.ExecuteAsync(id!);

        await act.Should().ThrowAsync<ArgumentException>();
    }
}
