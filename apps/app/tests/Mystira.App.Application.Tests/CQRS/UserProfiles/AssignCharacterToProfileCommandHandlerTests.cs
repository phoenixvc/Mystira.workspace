using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Mystira.Core.CQRS.UserProfiles.Commands;
using Mystira.Core.Ports.Data;
using Mystira.Domain.Models;
using Mystira.Domain.Enums;
using Mystira.Domain.ValueObjects;
using Mystira.Shared.Data.Repositories;

namespace Mystira.App.Application.Tests.CQRS.UserProfiles;

public class AssignCharacterToProfileCommandHandlerTests
{
    private readonly Mock<IUserProfileRepository> _profileRepository;
    private readonly Mock<ICharacterMapRepository> _characterRepository;
    private readonly Mock<IUnitOfWork> _unitOfWork;
    private readonly Mock<ILogger> _logger;

    public AssignCharacterToProfileCommandHandlerTests()
    {
        _profileRepository = new Mock<IUserProfileRepository>();
        _characterRepository = new Mock<ICharacterMapRepository>();
        _unitOfWork = new Mock<IUnitOfWork>();
        _logger = new Mock<ILogger>();
    }

    [Fact]
    public async Task Handle_WithValidProfileAndCharacter_ReturnsTrue()
    {
        var profile = new UserProfile { Id = "profile-1", Name = "Test" };
        var character = new CharacterMap { Id = "char-1", Name = "Elarion" };
        _profileRepository.Setup(r => r.GetByIdAsync("profile-1", It.IsAny<CancellationToken>())).ReturnsAsync(profile);
        _characterRepository.Setup(r => r.GetByIdAsync("char-1", It.IsAny<CancellationToken>())).ReturnsAsync(character);
        _unitOfWork.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        var result = await AssignCharacterToProfileCommandHandler.Handle(
            new AssignCharacterToProfileCommand("profile-1", "char-1"),
            _profileRepository.Object, _characterRepository.Object,
            _unitOfWork.Object, _logger.Object, CancellationToken.None);

        result.Should().BeTrue();
        _profileRepository.Verify(r => r.UpdateAsync(It.Is<UserProfile>(p => !p.IsNpc), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WithNpcFlag_SetsIsNpcTrue()
    {
        var profile = new UserProfile { Id = "profile-1", Name = "Test" };
        var character = new CharacterMap { Id = "char-1", Name = "NpcChar" };
        _profileRepository.Setup(r => r.GetByIdAsync("profile-1", It.IsAny<CancellationToken>())).ReturnsAsync(profile);
        _characterRepository.Setup(r => r.GetByIdAsync("char-1", It.IsAny<CancellationToken>())).ReturnsAsync(character);
        _unitOfWork.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        var result = await AssignCharacterToProfileCommandHandler.Handle(
            new AssignCharacterToProfileCommand("profile-1", "char-1", IsNpc: true),
            _profileRepository.Object, _characterRepository.Object,
            _unitOfWork.Object, _logger.Object, CancellationToken.None);

        result.Should().BeTrue();
        _profileRepository.Verify(r => r.UpdateAsync(It.Is<UserProfile>(p => p.IsNpc), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WithMissingProfile_ReturnsFalse()
    {
        _profileRepository.Setup(r => r.GetByIdAsync("missing", It.IsAny<CancellationToken>())).ReturnsAsync(default(UserProfile));

        var result = await AssignCharacterToProfileCommandHandler.Handle(
            new AssignCharacterToProfileCommand("missing", "char-1"),
            _profileRepository.Object, _characterRepository.Object,
            _unitOfWork.Object, _logger.Object, CancellationToken.None);

        result.Should().BeFalse();
    }

    [Fact]
    public async Task Handle_WithMissingCharacter_ReturnsFalse()
    {
        var profile = new UserProfile { Id = "profile-1", Name = "Test" };
        _profileRepository.Setup(r => r.GetByIdAsync("profile-1", It.IsAny<CancellationToken>())).ReturnsAsync(profile);
        _characterRepository.Setup(r => r.GetByIdAsync("missing", It.IsAny<CancellationToken>())).ReturnsAsync(default(CharacterMap));

        var result = await AssignCharacterToProfileCommandHandler.Handle(
            new AssignCharacterToProfileCommand("profile-1", "missing"),
            _profileRepository.Object, _characterRepository.Object,
            _unitOfWork.Object, _logger.Object, CancellationToken.None);

        result.Should().BeFalse();
    }
}
