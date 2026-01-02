using Ardalis.Specification;
using Ardalis.Specification.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Mystira.Application.Ports.Data;
using Mystira.Domain.Models;

namespace Mystira.Infrastructure.Data.Repositories;

/// <summary>
/// Repository implementation for BadgeImage entity with domain-specific queries.
/// Supports specification pattern for flexible querying.
/// </summary>
public class BadgeImageRepository : IBadgeImageRepository
{
    private readonly MystiraAppDbContext _context;

    /// <summary>
    /// Initializes a new instance of the <see cref="BadgeImageRepository"/> class.
    /// </summary>
    /// <param name="context">The database context.</param>
    public BadgeImageRepository(MystiraAppDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Retrieves a badge image by its unique identifier.
    /// </summary>
    /// <param name="id">The badge image ID.</param>
    /// <returns>The badge image, or null if not found.</returns>
    public async Task<BadgeImage?> GetByIdAsync(string id)
    {
        return await _context.BadgeImages.FirstOrDefaultAsync(x => x.Id == id);
    }

    /// <summary>
    /// Retrieves all badge images.
    /// </summary>
    /// <returns>A collection of all badge images.</returns>
    public async Task<IEnumerable<BadgeImage>> GetAllAsync()
    {
        return await _context.BadgeImages.ToListAsync();
    }

    /// <summary>
    /// Finds badge images matching the specified predicate.
    /// </summary>
    /// <param name="predicate">The filter predicate.</param>
    /// <returns>A collection of matching badge images.</returns>
    public async Task<IEnumerable<BadgeImage>> FindAsync(System.Linq.Expressions.Expression<Func<BadgeImage, bool>> predicate)
    {
        return await _context.BadgeImages.Where(predicate).ToListAsync();
    }

    /// <summary>
    /// Adds a new badge image to the repository.
    /// </summary>
    /// <param name="entity">The badge image to add.</param>
    /// <returns>The added badge image.</returns>
    public async Task<BadgeImage> AddAsync(BadgeImage entity)
    {
        await _context.BadgeImages.AddAsync(entity);
        return entity;
    }

    /// <summary>
    /// Updates an existing badge image in the repository.
    /// </summary>
    /// <param name="entity">The badge image to update.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public Task UpdateAsync(BadgeImage entity)
    {
        entity.UpdatedAt = DateTime.UtcNow;
        _context.BadgeImages.Update(entity);
        return Task.CompletedTask;
    }

    /// <summary>
    /// Deletes a badge image from the repository.
    /// </summary>
    /// <param name="id">The ID of the badge image to delete.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public async Task DeleteAsync(string id)
    {
        var entity = await GetByIdAsync(id);
        if (entity != null)
        {
            _context.BadgeImages.Remove(entity);
        }
    }

    /// <summary>
    /// Checks if a badge image with the specified ID exists.
    /// </summary>
    /// <param name="id">The badge image ID to check.</param>
    /// <returns>True if the badge image exists; otherwise, false.</returns>
    public async Task<bool> ExistsAsync(string id)
    {
        return await _context.BadgeImages.AnyAsync(x => x.Id == id);
    }

    /// <summary>
    /// Retrieves a single badge image matching the specification.
    /// </summary>
    /// <param name="spec">The specification to apply.</param>
    /// <returns>The badge image, or null if not found.</returns>
    public async Task<BadgeImage?> GetBySpecAsync(ISpecification<BadgeImage> spec)
    {
        return await ApplySpecification(spec).FirstOrDefaultAsync();
    }

    /// <summary>
    /// Retrieves all badge images matching the specification.
    /// </summary>
    /// <param name="spec">The specification to apply.</param>
    /// <returns>A collection of matching badge images.</returns>
    public async Task<IEnumerable<BadgeImage>> ListAsync(ISpecification<BadgeImage> spec)
    {
        return await ApplySpecification(spec).ToListAsync();
    }

    /// <summary>
    /// Counts badge images matching the specification.
    /// </summary>
    /// <param name="spec">The specification to apply.</param>
    /// <returns>The count of matching badge images.</returns>
    public async Task<int> CountAsync(ISpecification<BadgeImage> spec)
    {
        return await ApplySpecification(spec).CountAsync();
    }

    /// <summary>
    /// Retrieves a badge image by its ID.
    /// </summary>
    /// <param name="imageId">The image ID.</param>
    /// <returns>The badge image, or null if not found.</returns>
    public async Task<BadgeImage?> GetByImageIdAsync(string imageId)
    {
        // BadgeImage uses Id as its identifier (there's no separate ImageId property)
        return await _context.BadgeImages
            .FirstOrDefaultAsync(x => x.Id == imageId);
    }

    private IQueryable<BadgeImage> ApplySpecification(ISpecification<BadgeImage> spec)
    {
        return SpecificationEvaluator.Default.GetQuery(_context.BadgeImages.AsQueryable(), spec);
    }
}
