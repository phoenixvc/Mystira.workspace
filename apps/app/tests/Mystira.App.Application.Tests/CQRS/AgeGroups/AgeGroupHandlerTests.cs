using Mystira.Shared.Exceptions;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Mystira.Core.CQRS.AgeGroups.Commands;
using Mystira.Core.CQRS.AgeGroups.Queries;
using Mystira.Core.Ports.Data;
using Mystira.Core.Services;
using Mystira.Domain.Models;
using Mystira.Domain.Enums;
using Mystira.Domain.ValueObjects;
using Mystira.Shared.Data.Repositories;

namespace Mystira.App.Application.Tests.CQRS.AgeGroups;

public class AgeGroupHandlerTests
{
    private readonly Mock<IAgeGroupRepository> _repository;
    private readonly Mock<IUnitOfWork> _unitOfWork;
    private readonly Mock<IQueryCacheInvalidationService> _cacheInvalidation;
    private readonly Mock<ILogger> _logger;

    public AgeGroupHandlerTests()
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
        _cacheInvalidation.Verify(c => c.InvalidateCacheByPrefixAsync("MasterData:AgeGroups"), Times.Once);
    }

    [Fact]
    public async Task Create_WithEmptyName_ThrowsValidationException()
    {
        var command = new CreateAgeGroupCommand("", "6-9", 6, 9, "Description");

        var act = () => CreateAgeGroupCommandHandler.Handle(
            command, _repository.Object, _unitOfWork.Object,
            _cacheInvalidation.Object, _logger.Object, CancellationToken.None);

        await act.Should().ThrowAsync<ValidationException>();
    }

    [Fact]
    public async Task Create_WithEmptyValue_ThrowsValidationException()
    {
        var command = new CreateAgeGroupCommand("Young Explorers", "", 6, 9, "Description");

        var act = () => CreateAgeGroupCommandHandler.Handle(
            command, _repository.Object, _unitOfWork.Object,
            _cacheInvalidation.Object, _logger.Object, CancellationToken.None);

        await act.Should().ThrowAsync<ValidationException>();
    }

    #endregion

    #region DeleteAgeGroupCommandHandler Tests

    [Fact]
    public async Task Delete_WithExistingId_ReturnsTrue()
    {
        var ageGroup = new AgeGroupDefinition { Id = "ag-1", Name = "Young Explorers", Value = "6-9" };
        _repository.Setup(r => r.GetByIdAsync("ag-1", It.IsAny<CancellationToken>()))
            .ReturnsAsync(ageGroup);

        var result = await DeleteAgeGroupCommandHandler.Handle(
            new DeleteAgeGroupCommand("ag-1"), _repository.Object, _unitOfWork.Object,
            _cacheInvalidation.Object, _logger.Object, CancellationToken.None);

        result.Should().BeTrue();
        _repository.Verify(r => r.DeleteAsync("ag-1", It.IsAny<CancellationToken>()), Times.Once);
        _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        _cacheInvalidation.Verify(c => c.InvalidateCacheByPrefixAsync("MasterData:AgeGroups"), Times.Once);
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

    #region UpdateAgeGroupCommandHandler Tests

    [Fact]
    public async Task Update_WithExistingId_ReturnsUpdatedAgeGroup()
    {
        var existing = new AgeGroupDefinition
        {
            Id = "ag-1", Name = "Old", Value = "1-5",
            MinimumAge = 1, MaximumAge = 5, Description = "Old desc"
        };
        _repository.Setup(r => r.GetByIdAsync("ag-1", It.IsAny<CancellationToken>()))
            .ReturnsAsync(existing);

        var result = await UpdateAgeGroupCommandHandler.Handle(
            new UpdateAgeGroupCommand("ag-1", "Updated", "6-9", 6, 9, "New desc"),
            _repository.Object, _unitOfWork.Object,
            _cacheInvalidation.Object, _logger.Object, CancellationToken.None);

        result.Should().NotBeNull();
        result!.Name.Should().Be("Updated");
        result.Value.Should().Be("6-9");
        result.MinimumAge.Should().Be(6);
        result.MaximumAge.Should().Be(9);
        result.Description.Should().Be("New desc");
        _repository.Verify(r => r.UpdateAsync(existing, It.IsAny<CancellationToken>()), Times.Once);
        _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        _cacheInvalidation.Verify(c => c.InvalidateCacheByPrefixAsync("MasterData:AgeGroups"), Times.Once);
    }

    [Fact]
    public async Task Update_WithNonExistingId_ReturnsNull()
    {
        _repository.Setup(r => r.GetByIdAsync("missing", It.IsAny<CancellationToken>()))
            .ReturnsAsync(default(AgeGroupDefinition));

        var result = await UpdateAgeGroupCommandHandler.Handle(
            new UpdateAgeGroupCommand("missing", "Name", "6-9", 6, 9, "Desc"),
            _repository.Object, _unitOfWork.Object,
            _cacheInvalidation.Object, _logger.Object, CancellationToken.None);

        result.Should().BeNull();
        _repository.Verify(r => r.UpdateAsync(It.IsAny<AgeGroupDefinition>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    #endregion

    #region ValidateAgeGroupQueryHandler Tests

    [Fact]
    public async Task Validate_WithExistingValue_ReturnsTrue()
    {
        _repository.Setup(r => r.ExistsByValueAsync("6-9", It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var result = await ValidateAgeGroupQueryHandler.Handle(
            new ValidateAgeGroupQuery("6-9"), _repository.Object, _logger.Object, CancellationToken.None);

        result.Should().BeTrue();
    }

    [Fact]
    public async Task Validate_WithNonExistingValue_ReturnsFalse()
    {
        _repository.Setup(r => r.ExistsByValueAsync("99-100", It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        var result = await ValidateAgeGroupQueryHandler.Handle(
            new ValidateAgeGroupQuery("99-100"), _repository.Object, _logger.Object, CancellationToken.None);

        result.Should().BeFalse();
    }

    #endregion
}
