using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Mystira.App.Application.Ports.Data;
using Mystira.App.Application.UseCases.Avatars;
using Mystira.Domain.Models;
using Mystira.Domain.Enums;
using Mystira.Domain.ValueObjects;

namespace Mystira.App.Application.Tests.UseCases.Avatars;

public class UpdateAvatarConfigurationUseCaseTests
{
    private readonly Mock<IAvatarConfigurationFileRepository> _repository;
    private readonly Mock<IUnitOfWork> _unitOfWork;
    private readonly Mock<ILogger<UpdateAvatarConfigurationUseCase>> _logger;
    private readonly UpdateAvatarConfigurationUseCase _useCase;

    public UpdateAvatarConfigurationUseCaseTests()
    {
        _repository = new Mock<IAvatarConfigurationFileRepository>();
        _unitOfWork = new Mock<IUnitOfWork>();
        _logger = new Mock<ILogger<UpdateAvatarConfigurationUseCase>>();
        _useCase = new UpdateAvatarConfigurationUseCase(_repository.Object, _unitOfWork.Object, _logger.Object);
    }

    [Fact]
    public async Task ExecuteAsync_WithValidInput_UpdatesConfiguration()
    {
        _repository.Setup(r => r.AddOrUpdateAsync(It.IsAny<AvatarConfigurationFile>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((AvatarConfigurationFile config, CancellationToken _) => config);

        var ageGroupAvatars = new Dictionary<string, List<string>>
        {
            { "6-9", new List<string> { "new-avatar-1" } }
        };

        var result = await _useCase.ExecuteAsync(ageGroupAvatars);

        result.Should().NotBeNull();
        _repository.Verify(r => r.AddOrUpdateAsync(It.IsAny<AvatarConfigurationFile>(), It.IsAny<CancellationToken>()), Times.Once);
        _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
}
