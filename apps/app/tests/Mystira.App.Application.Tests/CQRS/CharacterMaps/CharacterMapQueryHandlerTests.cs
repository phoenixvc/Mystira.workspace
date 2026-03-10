using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Mystira.App.Application.CQRS.CharacterMaps.Queries;
using Mystira.App.Application.Ports.Data;
using Mystira.Domain.Models;
using Mystira.Domain.Enums;
using Mystira.Domain.ValueObjects;
using Mystira.Shared.Data.Repositories;

namespace Mystira.App.Application.Tests.CQRS.CharacterMaps;

public class CharacterMapQueryHandlerTests
{
    private readonly Mock<ICharacterMapRepository> _repository;
    private readonly Mock<IUnitOfWork> _unitOfWork;
    private readonly Mock<ILogger<GetAllCharacterMapsQuery>> _getAllLogger;
    private readonly Mock<ILogger<GetCharacterMapQuery>> _getByIdLogger;

    public CharacterMapQueryHandlerTests()
    {
        _repository = new Mock<ICharacterMapRepository>();
        _unitOfWork = new Mock<IUnitOfWork>();
        _getAllLogger = new Mock<ILogger<GetAllCharacterMapsQuery>>();
        _getByIdLogger = new Mock<ILogger<GetCharacterMapQuery>>();
    }

    #region GetAllCharacterMapsQueryHandler Tests

    [Fact]
    public async Task GetAll_WithExistingMaps_ReturnsMaps()
    {
        var maps = new List<CharacterMap>
        {
            new() { Id = "elarion", Name = "Elarion the Wise" },
            new() { Id = "grubb", Name = "Grubb the Goblin" }
        };
        _repository.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(maps);

        var result = await GetAllCharacterMapsQueryHandler.Handle(
            new GetAllCharacterMapsQuery(), _repository.Object, _unitOfWork.Object,
            _getAllLogger.Object, CancellationToken.None);

        result.Should().HaveCount(2);
        result.Should().Contain(m => m.Name == "Elarion the Wise");
    }

    [Fact]
    public async Task GetAll_WhenEmpty_InitializesDefaultsAndReturns()
    {
        var callCount = 0;
        _repository.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(() =>
            {
                callCount++;
                if (callCount == 1) return new List<CharacterMap>();
                return new List<CharacterMap>
                {
                    new() { Id = "elarion", Name = "Elarion the Wise" },
                    new() { Id = "grubb", Name = "Grubb the Goblin" }
                };
            });

        var result = await GetAllCharacterMapsQueryHandler.Handle(
            new GetAllCharacterMapsQuery(), _repository.Object, _unitOfWork.Object,
            _getAllLogger.Object, CancellationToken.None);

        result.Should().HaveCount(2);
        _repository.Verify(r => r.AddAsync(It.IsAny<CharacterMap>(), It.IsAny<CancellationToken>()), Times.Exactly(2));
        _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    #endregion

    #region GetCharacterMapQueryHandler Tests

    [Fact]
    public async Task GetById_WithExistingId_ReturnsCharacterMap()
    {
        var map = new CharacterMap { Id = "elarion", Name = "Elarion the Wise" };
        _repository.Setup(r => r.GetByIdAsync("elarion", It.IsAny<CancellationToken>()))
            .ReturnsAsync(map);

        var result = await GetCharacterMapQueryHandler.Handle(
            new GetCharacterMapQuery("elarion"), _repository.Object, _getByIdLogger.Object, CancellationToken.None);

        result.Should().NotBeNull();
        result!.Name.Should().Be("Elarion the Wise");
    }

    [Fact]
    public async Task GetById_WithNonExistingId_ReturnsNull()
    {
        _repository.Setup(r => r.GetByIdAsync("missing", It.IsAny<CancellationToken>()))
            .ReturnsAsync(default(CharacterMap));

        var result = await GetCharacterMapQueryHandler.Handle(
            new GetCharacterMapQuery("missing"), _repository.Object, _getByIdLogger.Object, CancellationToken.None);

        result.Should().BeNull();
    }

    #endregion
}
