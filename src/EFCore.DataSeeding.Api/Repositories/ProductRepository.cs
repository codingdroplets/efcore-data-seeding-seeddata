using EFCore.DataSeeding.Api.Data;
using EFCore.DataSeeding.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace EFCore.DataSeeding.Api.Repositories;

/// <summary>
/// EF Core-backed implementation of <see cref="IProductRepository"/>.
/// Uses AsNoTracking for all read-only queries to improve performance.
/// </summary>
public class ProductRepository : IProductRepository
{
    private readonly AppDbContext _db;

    public ProductRepository(AppDbContext db) => _db = db;

    public async Task<IReadOnlyList<Product>> GetAllAsync(CancellationToken cancellationToken = default)
        => await _db.Products
            .AsNoTracking()
            .Include(p => p.Category)
            .OrderBy(p => p.Name)
            .ToListAsync(cancellationToken);

    public async Task<Product?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
        => await _db.Products
            .AsNoTracking()
            .Include(p => p.Category)
            .FirstOrDefaultAsync(p => p.Id == id, cancellationToken);

    public async Task<IReadOnlyList<Product>> GetByCategorySlugAsync(string slug, CancellationToken cancellationToken = default)
        => await _db.Products
            .AsNoTracking()
            .Include(p => p.Category)
            .Where(p => p.Category.Slug == slug && p.IsActive)
            .OrderBy(p => p.Name)
            .ToListAsync(cancellationToken);
}
