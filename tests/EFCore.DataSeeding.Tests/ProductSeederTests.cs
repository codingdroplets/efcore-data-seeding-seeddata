using EFCore.DataSeeding.Api.Data;
using EFCore.DataSeeding.Api.Data.Seeding;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;

namespace EFCore.DataSeeding.Tests;

/// <summary>
/// Tests for Strategy 2: Custom runtime ProductSeeder.
/// </summary>
public class ProductSeederTests : IDisposable
{
    private readonly AppDbContext _db;
    private readonly ProductSeeder _seeder;

    public ProductSeederTests()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase($"SeederTest_{Guid.NewGuid()}")
            .Options;

        _db = new AppDbContext(options);
        _db.Database.EnsureCreated(); // apply HasData (categories + base products)

        _seeder = new ProductSeeder(_db, NullLogger<ProductSeeder>.Instance);
    }

    [Fact]
    public async Task ProductSeeder_InsertsHighStockProducts()
    {
        await _seeder.SeedAsync();

        var highStockCount = await _db.Products.CountAsync(p => p.Stock > 500);
        Assert.Equal(4, highStockCount);
    }

    [Fact]
    public async Task ProductSeeder_IsIdempotent_DoesNotDuplicate()
    {
        // Run twice
        await _seeder.SeedAsync();
        await _seeder.SeedAsync();

        var highStockCount = await _db.Products.CountAsync(p => p.Stock > 500);
        Assert.Equal(4, highStockCount); // still 4, not 8
    }

    [Fact]
    public async Task ProductSeeder_AssignsCorrectCategories()
    {
        await _seeder.SeedAsync();

        var allProducts = await _db.Products
            .Include(p => p.Category)
            .Where(p => p.Stock > 500)
            .ToListAsync();

        Assert.All(allProducts, p => Assert.NotNull(p.Category));
    }

    public void Dispose() => _db.Dispose();
}
