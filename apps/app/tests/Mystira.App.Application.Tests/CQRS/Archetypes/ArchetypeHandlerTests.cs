using Mystira.Shared.Exceptions;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Mystira.App.Application.CQRS.Archetypes.Commands;
using Mystira.App.Application.CQRS.Archetypes.Queries;
using Mystira.App.Application.Ports.Data;
using Mystira.App.Application.Services;
using Mystira.Domain.Models;
using Mystira.Domain.Enums;
using Mystira.Domain.ValueObjects;
using Mystira.Shared.Data.Repositories;

namespace Mystira.App.Application.Tests.CQRS.Archetypes;

public class ArchetypeHandlerTests
{
    private readonly Mock<IArchetypeRepository> _repository;
    private readonly Mock<IUnitOfWork> _unitOfWork;
    private readonly Mock<IQueryCacheInvalidationService> _cacheInvalidation;
    private readonly Mock<ILogger> _logger;

    public ArchetypeHandlerTests()
    {
        _repository = new Mock<IArchetypeRepository>();
        _unitOfWork = new Mock<IUnitOfWork>();
        _cacheInvalidation = new Mock<IQueryCacheInvalidationService>();
        _logger = new Mock<ILogger>();
    }

    #region CreateArchetypeCommandHandler Tests

    [Fact]
    public async Task Create_WithValidData_ReturnsCreatedArchetype()
    {
        var command = new CreateArchetypeCommand("Hero", "A brave hero archetype");

        var result = await CreateArchetypeCommandHandler.Handle(
            command, _repository.Object, _unitOfWork.Object,
            _cacheInvalidation.Object, _logger.Object, CancellationToken.None);

        result.Should().NotBeNull();
        result.Name.Should().Be("Hero");
        result.Description.Should().Be("A brave hero archetype");
        result.Id.Should().NotBeNullOrEmpty();
        _repository.Verify(r => r.AddAsync(It.IsAny<ArchetypeDefinition>(), It.IsAny<CancellationToken>()), Times.Once);
        _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        _cacheInvalidation.Verify(c => c.InvalidateCacheByPrefixAsync("MasterData:Archetypes"), Times.Once);
    }

    [Fact]
    public async Task Create_WithEmptyName_ThrowsValidationException()
    {
        var command = new CreateArchetypeCommand("", "Description");

        var act = () => CreateArchetypeCommandHandler.Handle(
            command, _repository.Object, _unitOfWork.Object,
            _cacheInvalidation.Object, _logger.Object, CancellationToken.None);

        await act.Should().ThrowAsync<ValidationException>();
    }

    #endregion

    #region DeleteArchetypeCommandHandler Tests

    [Fact]
    public async Task Delete_WithExistingId_ReturnsTrue()
    {
        var archetype = new ArchetypeDefinition { Id = "arch-1", Name = "Hero" };
        _repository.Setup(r => r.GetByIdAsync("arch-1", It.IsAny<CancellationToken>()))
            .ReturnsAsync(archetype);

        var command = new DeleteArchetypeCommand("arch-1");

        var result = await DeleteArchetypeCommandHandler.Handle(
            command, _repository.Object, _unitOfWork.Object,
            _cacheInvalidation.Object, _logger.Object, CancellationToken.None);

        result.Should().BeTrue();
        _repository.Verify(r => r.DeleteAsync("arch-1", It.IsAny<CancellationToken>()), Times.Once);
        _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        _cacheInvalidation.Verify(c => c.InvalidateCacheByPrefixAsync("MasterData:Archetypes"), Times.Once);
    }

    [Fact]
    public async Task Delete_WithNonExistingId_ReturnsFalse()
    {
        _repository.Setup(r => r.GetByIdAsync("missing", It.IsAny<CancellationToken>()))
            .ReturnsAsync(default(ArchetypeDefinition));

        var command = new DeleteArchetypeCommand("missing");

        var result = await DeleteArchetypeCommandHandler.Handle(
            command, _repository.Object, _unitOfWork.Object,
            _cacheInvalidation.Object, _logger.Object, CancellationToken.None);

        result.Should().BeFalse();
        _repository.Verify(r => r.DeleteAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    #endregion

    #region UpdateArchetypeCommandHandler Tests

    [Fact]
    public async Task Update_WithExistingId_ReturnsUpdatedArchetype()
    {
        var existing = new ArchetypeDefinition { Id = "arch-1", Name = "Old", Description = "Old desc" };
        _repository.Setup(r => r.GetByIdAsync("arch-1", It.IsAny<CancellationToken>()))
            .ReturnsAsync(existing);

        var command = new UpdateArchetypeCommand("arch-1", "New Name", "New desc");

        var result = await UpdateArchetypeCommandHandler.Handle(
            command, _repository.Object, _unitOfWork.Object,
            _cacheInvalidation.Object, _logger.Object, CancellationToken.None);

        result.Should().NotBeNull();
        result!.Name.Should().Be("New Name");
        result.Description.Should().Be("New desc");
        _repository.Verify(r => r.UpdateAsync(existing, It.IsAny<CancellationToken>()), Times.Once);
        _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Update_WithNonExistingId_ReturnsNull()
    {
        _repository.Setup(r => r.GetByIdAsync("missing", It.IsAny<CancellationToken>()))
            .ReturnsAsync(default(ArchetypeDefinition));

        var command = new UpdateArchetypeCommand("missing", "Name", "Desc");

        var result = await UpdateArchetypeCommandHandler.Handle(
            command, _repository.Object, _unitOfWork.Object,
            _cacheInvalidation.Object, _logger.Object, CancellationToken.None);

        result.Should().BeNull();
        _repository.Verify(r => r.UpdateAsync(It.IsAny<ArchetypeDefinition>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    #endregion

    #region GetAllArchetypesQueryHandler Tests

    [Fact]
    public async Task GetAll_ReturnsAllArchetypes()
    {
        var archetypes = new List<ArchetypeDefinition>
        {
            new() { Id = "1", Name = "Hero" },
            new() { Id = "2", Name = "Villain" }
        };
        _repository.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(archetypes);

        var result = await GetAllArchetypesQueryHandler.Handle(
            new GetAllArchetypesQuery(), _repository.Object, _logger.Object, CancellationToken.None);

        result.Should().HaveCount(2);
        result.Should().Contain(a => a.Name == "Hero");
    }

    [Fact]
    public async Task GetAll_WhenEmpty_ReturnsEmptyList()
    {
        _repository.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<ArchetypeDefinition>());

        var result = await GetAllArchetypesQueryHandler.Handle(
            new GetAllArchetypesQuery(), _repository.Object, _logger.Object, CancellationToken.None);

        result.Should().BeEmpty();
    }

    #endregion

    #region GetArchetypeByIdQueryHandler Tests

    [Fact]
    public async Task GetById_WithExistingId_ReturnsArchetype()
    {
        var archetype = new ArchetypeDefinition { Id = "arch-1", Name = "Hero" };
        _repository.Setup(r => r.GetByIdAsync("arch-1", It.IsAny<CancellationToken>()))
            .ReturnsAsync(archetype);

        var result = await GetArchetypeByIdQueryHandler.Handle(
            new GetArchetypeByIdQuery("arch-1"), _repository.Object, _logger.Object, CancellationToken.None);

        result.Should().NotBeNull();
        result!.Name.Should().Be("Hero");
    }

    [Fact]
    public async Task GetById_WithNonExistingId_ReturnsNull()
    {
        _repository.Setup(r => r.GetByIdAsync("missing", It.IsAny<CancellationToken>()))
            .ReturnsAsync(default(ArchetypeDefinition));

        var result = await GetArchetypeByIdQueryHandler.Handle(
            new GetArchetypeByIdQuery("missing"), _repository.Object, _logger.Object, CancellationToken.None);

        result.Should().BeNull();
    }

    #endregion

    #region ValidateArchetypeQueryHandler Tests

    [Fact]
    public async Task Validate_WithExistingName_ReturnsTrue()
    {
        _repository.Setup(r => r.ExistsByNameAsync("Hero", It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var result = await ValidateArchetypeQueryHandler.Handle(
            new ValidateArchetypeQuery("Hero"), _repository.Object, _logger.Object, CancellationToken.None);

        result.Should().BeTrue();
    }

    [Fact]
    public async Task Validate_WithNonExistingName_ReturnsFalse()
    {
        _repository.Setup(r => r.ExistsByNameAsync("Unknown", It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        var result = await ValidateArchetypeQueryHandler.Handle(
            new ValidateArchetypeQuery("Unknown"), _repository.Object, _logger.Object, CancellationToken.None);

        result.Should().BeFalse();
    }

    #endregion

    #region Cache Key Tests

    [Fact]
    public void GetAllArchetypesQuery_HasCorrectCacheKey()
    {
        var query = new GetAllArchetypesQuery();
        query.CacheKey.Should().Be("MasterData:Archetypes:All");
    }

    [Fact]
    public void GetArchetypeByIdQuery_HasCorrectCacheKey()
    {
        var query = new GetArchetypeByIdQuery("test-id");
        query.CacheKey.Should().Be("MasterData:Archetypes:test-id");
    }

    #endregion

    #region Timestamp Tests

    [Fact]
    public async Task Create_SetsTimestamps()
    {
        var before = DateTime.UtcNow;
        var command = new CreateArchetypeCommand("Mentor", "Wise guide");

        var result = await CreateArchetypeCommandHandler.Handle(
            command, _repository.Object, _unitOfWork.Object,
            _cacheInvalidation.Object, _logger.Object, CancellationToken.None);

        result.CreatedAt.Should().BeOnOrAfter(before);
        result.UpdatedAt.Should().BeOnOrAfter(before);
    }

    [Fact]
    public async Task Update_SetsUpdatedTimestamp()
    {
        var existing = new ArchetypeDefinition
        {
            Id = "arch-1", Name = "Old", UpdatedAt = DateTime.UtcNow.AddDays(-1)
        };
        _repository.Setup(r => r.GetByIdAsync("arch-1", It.IsAny<CancellationToken>()))
            .ReturnsAsync(existing);

        var before = DateTime.UtcNow;

        var result = await UpdateArchetypeCommandHandler.Handle(
            new UpdateArchetypeCommand("arch-1", "New", "Desc"),
            _repository.Object, _unitOfWork.Object,
            _cacheInvalidation.Object, _logger.Object, CancellationToken.None);

        result!.UpdatedAt.Should().BeOnOrAfter(before);
    }

    #endregion

    #region Cache Invalidation Tests

    [Fact]
    public async Task Update_InvalidatesCache()
    {
        var existing = new ArchetypeDefinition { Id = "arch-1", Name = "Old" };
        _repository.Setup(r => r.GetByIdAsync("arch-1", It.IsAny<CancellationToken>()))
            .ReturnsAsync(existing);

        await UpdateArchetypeCommandHandler.Handle(
            new UpdateArchetypeCommand("arch-1", "New", "Desc"),
            _repository.Object, _unitOfWork.Object,
            _cacheInvalidation.Object, _logger.Object, CancellationToken.None);

        _cacheInvalidation.Verify(c => c.InvalidateCacheByPrefixAsync("MasterData:Archetypes"), Times.Once);
    }

    [Fact]
    public async Task Delete_InvalidatesCache()
    {
        var existing = new ArchetypeDefinition { Id = "arch-1", Name = "Hero" };
        _repository.Setup(r => r.GetByIdAsync("arch-1", It.IsAny<CancellationToken>()))
            .ReturnsAsync(existing);

        await DeleteArchetypeCommandHandler.Handle(
            new DeleteArchetypeCommand("arch-1"), _repository.Object, _unitOfWork.Object,
            _cacheInvalidation.Object, _logger.Object, CancellationToken.None);

        _cacheInvalidation.Verify(c => c.InvalidateCacheByPrefixAsync("MasterData:Archetypes"), Times.Once);
    }

    #endregion
}
