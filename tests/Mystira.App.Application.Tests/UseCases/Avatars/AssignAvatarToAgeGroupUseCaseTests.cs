using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Mystira.App.Application.Ports.Data;
using Mystira.App.Application.UseCases.Avatars;
using Mystira.App.Domain.Models;

namespace Mystira.App.Application.Tests.UseCases.Avatars;

public class AssignAvatarToAgeGroupUseCaseTests
{
    private readonly Mock<IAvatarConfigurationFileRepository> _repository;
    private readonly Mock<IUnitOfWork> _unitOfWork;
    private readonly Mock<ILogger<AssignAvatarToAgeGroupUseCase>> _logger;
    private readonly AssignAvatarToAgeGroupUseCase _useCase;

    public AssignAvatarToAgeGroupUseCaseTests()
    {
        _repository = new Mock<IAvatarConfigurationFileRepository>();
        _unitOfWork = new Mock<IUnitOfWork>();
        _logger = new Mock<ILogger<AssignAvatarToAgeGroupUseCase>>();
        _useCase = new AssignAvatarToAgeGroupUseCase(_repository.Object, _unitOfWork.Object, _logger.Object);
    }

    [Fact]
    public async Task ExecuteAsync_WithValidInput_AssignsAvatars()
    {
        var existing = new AvatarConfigurationFile
        {
            AgeGroupAvatars = new Dictionary<string, List<string>>
            {
                { "6-9", new List<string> { "old-avatar" } }
            }
        };
        _repository.Setup(r => r.GetAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(existing);

        var mediaIds = new List<string> { "new-avatar-1", "new-avatar-2" };

        var result = await _useCase.ExecuteAsync("6-9", mediaIds);

        result.Should().NotBeNull();
        _repository.Verify(r => r.AddOrUpdateAsync(It.IsAny<AvatarConfigurationFile>(), It.IsAny<CancellationToken>()), Times.Once);
        _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public async Task ExecuteAsync_WithNullOrEmptyAgeGroup_ThrowsArgumentException(string? ageGroup)
    {
        var act = () => _useCase.ExecuteAsync(ageGroup!, new List<string> { "avatar-1" });

        await act.Should().ThrowAsync<ArgumentException>();
    }
}
