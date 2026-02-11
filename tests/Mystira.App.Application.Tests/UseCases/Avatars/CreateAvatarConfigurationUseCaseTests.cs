using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Mystira.App.Application.Ports.Data;
using Mystira.App.Application.UseCases.Avatars;
using Mystira.App.Domain.Models;

namespace Mystira.App.Application.Tests.UseCases.Avatars;

public class CreateAvatarConfigurationUseCaseTests
{
    private readonly Mock<IAvatarConfigurationFileRepository> _repository;
    private readonly Mock<IUnitOfWork> _unitOfWork;
    private readonly Mock<ILogger<CreateAvatarConfigurationUseCase>> _logger;
    private readonly CreateAvatarConfigurationUseCase _useCase;

    public CreateAvatarConfigurationUseCaseTests()
    {
        _repository = new Mock<IAvatarConfigurationFileRepository>();
        _unitOfWork = new Mock<IUnitOfWork>();
        _logger = new Mock<ILogger<CreateAvatarConfigurationUseCase>>();
        _useCase = new CreateAvatarConfigurationUseCase(_repository.Object, _unitOfWork.Object, _logger.Object);
    }

    [Fact]
    public async Task ExecuteAsync_WithValidInput_CreatesConfiguration()
    {
        var ageGroupAvatars = new Dictionary<string, List<string>>
        {
            { "6-9", new List<string> { "avatar-1", "avatar-2" } },
            { "10-12", new List<string> { "avatar-3" } }
        };

        var result = await _useCase.ExecuteAsync(ageGroupAvatars);

        result.Should().NotBeNull();
        result.AgeGroupAvatars.Should().HaveCount(2);
        _repository.Verify(r => r.AddOrUpdateAsync(It.IsAny<AvatarConfigurationFile>(), It.IsAny<CancellationToken>()), Times.Once);
        _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ExecuteAsync_WithEmptyDictionary_ThrowsArgumentException()
    {
        var act = () => _useCase.ExecuteAsync(new Dictionary<string, List<string>>());

        await act.Should().ThrowAsync<ArgumentException>();
    }

    [Fact]
    public async Task ExecuteAsync_WithNullInput_ThrowsArgumentException()
    {
        var act = () => _useCase.ExecuteAsync(null!);

        await act.Should().ThrowAsync<ArgumentException>();
    }
}
