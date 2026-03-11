using EFCore.DataSeeding.Api.Data;
using Microsoft.EntityFrameworkCore;

namespace EFCore.DataSeeding.Tests;

/// <summary>
/// Tests for Strategy 1: Model-based HasData seeding.
/// Uses InMemory + EnsureCreated to apply the HasData configuration.
/// </summary>
public class HasDataSeedingTests : IDisposable
{
    private readonly AppDbContext _db;

    public HasDataSeedingTests()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase($"HasDataTest_{Guid.NewGuid()}")
            .Options;

        _db = new AppDbContext(options);
        _db.Database.EnsureCreated(); // applies HasData seed
    }

    [Fact]
    public async Task HasData_Seeds_ThreeCategories()
    {
        var count = await _db.Categories.CountAsync();
        Assert.Equal(3, count);
    }

    [Fact]
    public async Task HasData_Seeds_ElectronicsCategory_WithCorrectSlug()
    {
        var category = await _db.Categories.FirstOrDefaultAsync(c => c.Slug == "electronics");
        Assert.NotNull(category);
        Assert.Equal("Electronics", category.Name);
    }

    [Fact]
    public async Task HasData_Seeds_FourBaseProducts()
    {
        var count = await _db.Products.CountAsync();
        Assert.Equal(4, count);
    }

    [Fact]
    public async Task HasData_Products_HaveCorrectCategoryAssignments()
    {
        var electronicProducts = await _db.Products
            .Where(p => p.CategoryId == 1)
            .ToListAsync();

        Assert.Equal(2, electronicProducts.Count);
    }

    public void Dispose() => _db.Dispose();
}
