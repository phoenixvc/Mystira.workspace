using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Mystira.App.Application.CQRS.AgeGroups.Commands;
using Mystira.App.Application.Ports.Data;
using Mystira.App.Application.Services;
using Mystira.App.Domain.Models;

namespace Mystira.App.Application.Tests.CQRS.AgeGroups;

public class AgeGroupCommandHandlerTests
{
    private readonly Mock<IAgeGroupRepository> _repository;
    private readonly Mock<IUnitOfWork> _unitOfWork;
    private readonly Mock<IQueryCacheInvalidationService> _cacheInvalidation;
    private readonly Mock<ILogger> _logger;

    public AgeGroupCommandHandlerTests()
    {
        _repository = new Mock<IAgeGroupRepository>();
        _unitOfWork = new Mock<IUnitOfWork>();
        _cacheInvalidation = new Mock<IQueryCacheInvalidationService>();
        _logger = new Mock<ILogger>();
    }

    #region CreateAgeGroupCommandHandler Tests

    [Fact]
    public async Task Create_WithValidData_ReturnsCreatedAgeGroup()
    {
        var command = new CreateAgeGroupCommand("Young Explorers", "6-9", 6, 9, "For children ages 6-9");

        var result = await CreateAgeGroupCommandHandler.Handle(
            command, _repository.Object, _unitOfWork.Object,
            _cacheInvalidation.Object, _logger.Object, CancellationToken.None);

        result.Should().NotBeNull();
        result.Name.Should().Be("Young Explorers");
        result.Value.Should().Be("6-9");
        result.MinimumAge.Should().Be(6);
        result.MaximumAge.Should().Be(9);
        result.Description.Should().Be("For children ages 6-9");
        result.Id.Should().NotBeNullOrEmpty();
        _repository.Verify(r => r.AddAsync(It.IsAny<AgeGroupDefinition>(), It.IsAny<CancellationToken>()), Times.Once);
        _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        _cacheInvalidation.Verify(c => c.InvalidateCacheByPrefix("MasterData:AgeGroups"), Times.Once);
    }

    [Fact]
    public async Task Create_SetsTimestamps()
    {
        var before = DateTime.UtcNow;
        var command = new CreateAgeGroupCommand("Teens", "13-17", 13, 17, "Teenagers");

        var result = await CreateAgeGroupCommandHandler.Handle(
            command, _repository.Object, _unitOfWork.Object,
            _cacheInvalidation.Object, _logger.Object, CancellationToken.None);

        result.CreatedAt.Should().BeOnOrAfter(before);
        result.UpdatedAt.Should().BeOnOrAfter(before);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public async Task Create_WithEmptyName_ThrowsArgumentException(string? name)
    {
        var command = new CreateAgeGroupCommand(name!, "6-9", 6, 9, "Description");

        var act = () => CreateAgeGroupCommandHandler.Handle(
            command, _repository.Object, _unitOfWork.Object,
            _cacheInvalidation.Object, _logger.Object, CancellationToken.None);

        await act.Should().ThrowAsync<ArgumentException>();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public async Task Create_WithEmptyValue_ThrowsArgumentException(string? value)
    {
        var command = new CreateAgeGroupCommand("Name", value!, 6, 9, "Description");

        var act = () => CreateAgeGroupCommandHandler.Handle(
            command, _repository.Object, _unitOfWork.Object,
            _cacheInvalidation.Object, _logger.Object, CancellationToken.None);

        await act.Should().ThrowAsync<ArgumentException>();
    }

    #endregion

    #region UpdateAgeGroupCommandHandler Tests

    [Fact]
    public async Task Update_WithExistingId_ReturnsUpdatedAgeGroup()
    {
        var existing = new AgeGroupDefinition
        {
            Id = "ag-1", Name = "Old Name", Value = "1-5",
            MinimumAge = 1, MaximumAge = 5, Description = "Old"
        };
        _repository.Setup(r => r.GetByIdAsync("ag-1", It.IsAny<CancellationToken>()))
            .ReturnsAsync(existing);

        var command = new UpdateAgeGroupCommand("ag-1", "Updated Name", "6-9", 6, 9, "Updated desc");

        var result = await UpdateAgeGroupCommandHandler.Handle(
            command, _repository.Object, _unitOfWork.Object,
            _cacheInvalidation.Object, _logger.Object, CancellationToken.None);

        result.Should().NotBeNull();
        result!.Name.Should().Be("Updated Name");
        result.Value.Should().Be("6-9");
        result.MinimumAge.Should().Be(6);
        result.MaximumAge.Should().Be(9);
        result.Description.Should().Be("Updated desc");
        _repository.Verify(r => r.UpdateAsync(existing, It.IsAny<CancellationToken>()), Times.Once);
        _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        _cacheInvalidation.Verify(c => c.InvalidateCacheByPrefix("MasterData:AgeGroups"), Times.Once);
    }

    [Fact]
    public async Task Update_WithNonExistingId_ReturnsNull()
    {
        _repository.Setup(r => r.GetByIdAsync("missing", It.IsAny<CancellationToken>()))
            .ReturnsAsync(default(AgeGroupDefinition));

        var command = new UpdateAgeGroupCommand("missing", "Name", "6-9", 6, 9, "Desc");

        var result = await UpdateAgeGroupCommandHandler.Handle(
            command, _repository.Object, _unitOfWork.Object,
            _cacheInvalidation.Object, _logger.Object, CancellationToken.None);

        result.Should().BeNull();
        _repository.Verify(r => r.UpdateAsync(It.IsAny<AgeGroupDefinition>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Update_SetsUpdatedTimestamp()
    {
        var existing = new AgeGroupDefinition
        {
            Id = "ag-1", Name = "Old", Value = "1-5",
            UpdatedAt = DateTime.UtcNow.AddDays(-1)
        };
        _repository.Setup(r => r.GetByIdAsync("ag-1", It.IsAny<CancellationToken>()))
            .ReturnsAsync(existing);

        var before = DateTime.UtcNow;
        var command = new UpdateAgeGroupCommand("ag-1", "New", "6-9", 6, 9, "Desc");

        var result = await UpdateAgeGroupCommandHandler.Handle(
            command, _repository.Object, _unitOfWork.Object,
            _cacheInvalidation.Object, _logger.Object, CancellationToken.None);

        result!.UpdatedAt.Should().BeOnOrAfter(before);
    }

    #endregion

    #region DeleteAgeGroupCommandHandler Tests

    [Fact]
    public async Task Delete_WithExistingId_ReturnsTrue()
    {
        var existing = new AgeGroupDefinition { Id = "ag-1", Name = "To Delete" };
        _repository.Setup(r => r.GetByIdAsync("ag-1", It.IsAny<CancellationToken>()))
            .ReturnsAsync(existing);

        var result = await DeleteAgeGroupCommandHandler.Handle(
            new DeleteAgeGroupCommand("ag-1"), _repository.Object, _unitOfWork.Object,
            _cacheInvalidation.Object, _logger.Object, CancellationToken.None);

        result.Should().BeTrue();
        _repository.Verify(r => r.DeleteAsync("ag-1", It.IsAny<CancellationToken>()), Times.Once);
        _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        _cacheInvalidation.Verify(c => c.InvalidateCacheByPrefix("MasterData:AgeGroups"), Times.Once);
    }

    [Fact]
    public async Task Delete_WithNonExistingId_ReturnsFalse()
    {
        _repository.Setup(r => r.GetByIdAsync("missing", It.IsAny<CancellationToken>()))
            .ReturnsAsync(default(AgeGroupDefinition));

        var result = await DeleteAgeGroupCommandHandler.Handle(
            new DeleteAgeGroupCommand("missing"), _repository.Object, _unitOfWork.Object,
            _cacheInvalidation.Object, _logger.Object, CancellationToken.None);

        result.Should().BeFalse();
        _repository.Verify(r => r.DeleteAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    #endregion

    #region ValidateAgeGroupQueryHandler Tests

    [Fact]
    public async Task Validate_WithExistingValue_ReturnsTrue()
    {
        _repository.Setup(r => r.ExistsByValueAsync("6-9", It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var result = await ValidateAgeGroupQueryHandler.Handle(
            new Application.CQRS.AgeGroups.Queries.ValidateAgeGroupQuery("6-9"),
            _repository.Object, _logger.Object, CancellationToken.None);

        result.Should().BeTrue();
    }

    [Fact]
    public async Task Validate_WithNonExistingValue_ReturnsFalse()
    {
        _repository.Setup(r => r.ExistsByValueAsync("unknown", It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        var result = await ValidateAgeGroupQueryHandler.Handle(
            new Application.CQRS.AgeGroups.Queries.ValidateAgeGroupQuery("unknown"),
            _repository.Object, _logger.Object, CancellationToken.None);

        result.Should().BeFalse();
    }

    #endregion
}
