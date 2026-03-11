using EFCore.DataSeeding.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace EFCore.DataSeeding.Api.Data.Seeding;

/// <summary>
/// Strategy 2: Custom runtime seeder.
///
/// Best for: large datasets, computed values, data pulled from external
///           sources, or scenarios where hard-coded IDs are undesirable.
///
/// Pros : full C# expressiveness, no migration bloat, IDs are DB-generated
/// Cons : runs at app startup (small overhead), must guard against duplicates
/// </summary>
public class ProductSeeder : IDataSeeder
{
    private readonly AppDbContext _db;
    private readonly ILogger<ProductSeeder> _logger;

    public int Order => 10; // Run after any lower-order seeders

    public ProductSeeder(AppDbContext db, ILogger<ProductSeeder> logger)
    {
        _db = db;
        _logger = logger;
    }

    public async Task SeedAsync(CancellationToken cancellationToken = default)
    {
        // ── Idempotency guard ────────────────────────────────────────────────
        // We only seed if there are no "runtime-seeded" products (stock > 500).
        // Adjust the guard predicate to match your domain logic.
        bool alreadySeeded = await _db.Products
            .AnyAsync(p => p.Stock > 500, cancellationToken);

        if (alreadySeeded)
        {
            _logger.LogInformation("ProductSeeder: data already present, skipping.");
            return;
        }

        _logger.LogInformation("ProductSeeder: seeding high-stock products...");

        // Fetch categories by slug so we are not hard-coding FK IDs
        var electronics = await _db.Categories
            .FirstAsync(c => c.Slug == "electronics", cancellationToken);

        var books = await _db.Categories
            .FirstAsync(c => c.Slug == "books", cancellationToken);

        var bulkProducts = new List<Product>
        {
            new() { Name = "USB-C Hub (7-in-1)",      Price = 49.99m,  Stock = 750,  CategoryId = electronics.Id },
            new() { Name = "Webcam 4K",                Price = 199.99m, Stock = 600,  CategoryId = electronics.Id },
            new() { Name = "The Pragmatic Programmer", Price = 44.99m,  Stock = 1200, CategoryId = books.Id },
            new() { Name = "Design Patterns (GoF)",    Price = 54.99m,  Stock = 800,  CategoryId = books.Id },
        };

        await _db.Products.AddRangeAsync(bulkProducts, cancellationToken);
        await _db.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("ProductSeeder: inserted {Count} products.", bulkProducts.Count);
    }
}
