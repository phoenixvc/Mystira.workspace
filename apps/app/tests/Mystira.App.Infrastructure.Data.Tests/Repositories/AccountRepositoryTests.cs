using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Mystira.Domain.Models;
using Mystira.Domain.Enums;
using Mystira.Domain.ValueObjects;
using Mystira.App.Infrastructure.Data;
using Mystira.App.Infrastructure.Data.Repositories;

namespace Mystira.App.Infrastructure.Data.Tests.Repositories;

public class AccountRepositoryTests : IDisposable
{
    private readonly MystiraAppDbContext _context;
    private readonly AccountRepository _repository;

    public AccountRepositoryTests()
    {
        var options = new DbContextOptionsBuilder<MystiraAppDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new MystiraAppDbContext(options);
        _repository = new AccountRepository(_context);
    }

    public void Dispose() => _context.Dispose();

    [Fact]
    public async Task GetByEmailAsync_WithExistingEmail_ReturnsAccount()
    {
        // Arrange
        var account = CreateTestAccount("acc-1", "test@example.com");
        await _context.Accounts.AddAsync(account);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetByEmailAsync("test@example.com");

        // Assert
        result.Should().NotBeNull();
        result!.Email.Should().Be("test@example.com");
    }

    [Fact]
    public async Task GetByEmailAsync_IsCaseInsensitive()
    {
        // Arrange
        var account = CreateTestAccount("acc-1", "Test@Example.com");
        await _context.Accounts.AddAsync(account);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetByEmailAsync("test@example.com");

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be("acc-1");
    }

    [Fact]
    public async Task GetByEmailAsync_WithNonExistingEmail_ReturnsNull()
    {
        var result = await _repository.GetByEmailAsync("nonexistent@example.com");
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetByExternalUserIdAsync_WithExistingId_ReturnsAccount()
    {
        // Arrange
        var account = CreateTestAccount("acc-1", "test@example.com", "ext-user-1");
        await _context.Accounts.AddAsync(account);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetByExternalUserIdAsync("ext-user-1");

        // Assert
        result.Should().NotBeNull();
        result!.ExternalUserId.Should().Be("ext-user-1");
    }

    [Fact]
    public async Task GetByExternalUserIdAsync_WithNonExistingId_ReturnsNull()
    {
        var result = await _repository.GetByExternalUserIdAsync("nonexistent");
        result.Should().BeNull();
    }

    [Fact]
    public async Task ExistsByEmailAsync_WithExistingEmail_ReturnsTrue()
    {
        // Arrange
        var account = CreateTestAccount("acc-1", "test@example.com");
        await _context.Accounts.AddAsync(account);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.ExistsByEmailAsync("test@example.com");

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task ExistsByEmailAsync_WithNonExistingEmail_ReturnsFalse()
    {
        var result = await _repository.ExistsByEmailAsync("nonexistent@example.com");
        result.Should().BeFalse();
    }

    [Fact]
    public async Task ExistsByEmailAsync_IsCaseInsensitive()
    {
        // Arrange
        var account = CreateTestAccount("acc-1", "Test@Example.COM");
        await _context.Accounts.AddAsync(account);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.ExistsByEmailAsync("test@example.com");

        // Assert
        result.Should().BeTrue();
    }

    private static Account CreateTestAccount(string id, string email, string? externalUserId = null)
    {
        return new Account
        {
            Id = id,
            Email = email,
            ExternalUserId = externalUserId ?? Guid.NewGuid().ToString(),
            DisplayName = "Test User",
            UserProfileIds = new List<string>(),
            CompletedScenarioIds = new List<string>(),
            Subscription = new SubscriptionDetails(),
            Settings = new AccountSettings(),
            CreatedAt = DateTime.UtcNow,
            LastLoginAt = DateTime.UtcNow
        };
    }
}
