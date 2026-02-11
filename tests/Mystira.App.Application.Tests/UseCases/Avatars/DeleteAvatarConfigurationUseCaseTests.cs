using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Mystira.App.Application.Ports.Data;
using Mystira.App.Application.UseCases.Avatars;

namespace Mystira.App.Application.Tests.UseCases.Avatars;

public class DeleteAvatarConfigurationUseCaseTests
{
    private readonly Mock<IAvatarConfigurationFileRepository> _repository;
    private readonly Mock<IUnitOfWork> _unitOfWork;
    private readonly Mock<ILogger<DeleteAvatarConfigurationUseCase>> _logger;
    private readonly DeleteAvatarConfigurationUseCase _useCase;

    public DeleteAvatarConfigurationUseCaseTests()
    {
        _repository = new Mock<IAvatarConfigurationFileRepository>();
        _unitOfWork = new Mock<IUnitOfWork>();
        _logger = new Mock<ILogger<DeleteAvatarConfigurationUseCase>>();
        _useCase = new DeleteAvatarConfigurationUseCase(_repository.Object, _unitOfWork.Object, _logger.Object);
    }

    [Fact]
    public async Task ExecuteAsync_DeletesConfiguration()
    {
        var result = await _useCase.ExecuteAsync();

        result.Should().BeTrue();
        _repository.Verify(r => r.DeleteAsync(It.IsAny<CancellationToken>()), Times.Once);
        _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
}
