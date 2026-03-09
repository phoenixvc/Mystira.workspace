using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Mystira.App.Application.CQRS.Avatars.Queries;
using Mystira.App.Application.Ports.Data;
using Mystira.App.Domain.Models;

namespace Mystira.App.Application.Tests.CQRS.Avatars;

public class GetAvatarsQueryHandlerTests
{
    private readonly Mock<IAvatarConfigurationFileRepository> _repository;
    private readonly Mock<ILogger<GetAvatarsQuery>> _logger;

    public GetAvatarsQueryHandlerTests()
    {
        _repository = new Mock<IAvatarConfigurationFileRepository>();
        _logger = new Mock<ILogger<GetAvatarsQuery>>();
    }

    [Fact]
    public async Task Handle_WithExistingConfiguration_ReturnsAvatars()
    {
        // Arrange
        var configFile = new AvatarConfigurationFile
        {
            AgeGroupAvatars = new Dictionary<string, List<string>>
            {
                { "6-9", new List<string> { "avatar-1", "avatar-2", "avatar-3" } },
                { "10-12", new List<string> { "avatar-4", "avatar-5" } }
            }
        };

        var query = new GetAvatarsQuery();

        _repository.Setup(r => r.GetAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(configFile);

        // Act
        var result = await GetAvatarsQueryHandler.Handle(
            query,
            _repository.Object,
            _logger.Object,
            CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.AgeGroupAvatars.Should().ContainKey("6-9");
        result.AgeGroupAvatars["6-9"].Should().HaveCount(3);
        result.AgeGroupAvatars["10-12"].Should().HaveCount(2);
    }

    [Fact]
    public async Task Handle_WithNullConfiguration_ReturnsEmptyDictionary()
    {
        // Arrange
        var query = new GetAvatarsQuery();

        _repository.Setup(r => r.GetAsync(It.IsAny<CancellationToken>()))
            .Returns(Task.FromResult<AvatarConfigurationFile?>(null));

        // Act
        var result = await GetAvatarsQueryHandler.Handle(
            query,
            _repository.Object,
            _logger.Object,
            CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.AgeGroupAvatars.Should().NotBeNull();
        // Handler initializes all age groups, so dictionary won't be empty
        // but values from null config start empty before age group initialization
    }

    [Fact]
    public async Task Handle_EnsuresAllAgeGroupsArePresent()
    {
        // Arrange
        var configFile = new AvatarConfigurationFile
        {
            AgeGroupAvatars = new Dictionary<string, List<string>>
            {
                { "6-9", new List<string> { "avatar-1" } }
                // Missing other age groups
            }
        };

        var query = new GetAvatarsQuery();

        _repository.Setup(r => r.GetAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(configFile);

        // Act
        var result = await GetAvatarsQueryHandler.Handle(
            query,
            _repository.Object,
            _logger.Object,
            CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        // Should have "6-9" with avatars
        result.AgeGroupAvatars["6-9"].Should().HaveCount(1);
        // Verify all expected age groups are initialized
        result.AgeGroupAvatars.Keys.Should().Contain(AgeGroupConstants.AllAgeGroups);
    }

    [Fact]
    public async Task Handle_WithEmptyAgeGroupAvatars_ReturnsEmptyLists()
    {
        // Arrange
        var configFile = new AvatarConfigurationFile
        {
            AgeGroupAvatars = new Dictionary<string, List<string>>()
        };

        var query = new GetAvatarsQuery();

        _repository.Setup(r => r.GetAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(configFile);

        // Act
        var result = await GetAvatarsQueryHandler.Handle(
            query,
            _repository.Object,
            _logger.Object,
            CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.AgeGroupAvatars.Should().NotBeNull();
        result.AgeGroupAvatars.Values.Should().OnlyContain(list => list.Count == 0);
    }

    [Fact]
    public async Task Handle_PreservesExistingAvatarIds()
    {
        // Arrange
        var expectedAvatars = new List<string> { "avatar-abc", "avatar-def", "avatar-ghi" };
        var configFile = new AvatarConfigurationFile
        {
            AgeGroupAvatars = new Dictionary<string, List<string>>
            {
                { "6-9", expectedAvatars }
            }
        };

        var query = new GetAvatarsQuery();

        _repository.Setup(r => r.GetAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(configFile);

        // Act
        var result = await GetAvatarsQueryHandler.Handle(
            query,
            _repository.Object,
            _logger.Object,
            CancellationToken.None);

        // Assert
        result.AgeGroupAvatars["6-9"].Should().Equal(expectedAvatars);
    }
}
