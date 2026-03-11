using EFCore.DataSeeding.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace EFCore.DataSeeding.Api.Data;

/// <summary>
/// Application database context.
/// Demonstrates multiple EF Core data-seeding strategies side by side.
/// </summary>
public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Category> Categories => Set<Category>();
    public DbSet<Product> Products => Set<Product>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // ─── Entity configuration ────────────────────────────────────────────
        modelBuilder.Entity<Category>(entity =>
        {
            entity.HasKey(c => c.Id);
            entity.HasIndex(c => c.Slug).IsUnique();
            entity.Property(c => c.Slug).HasMaxLength(100).IsRequired();
            entity.Property(c => c.Name).HasMaxLength(200).IsRequired();
            entity.Property(c => c.Description).HasMaxLength(500);
        });

        modelBuilder.Entity<Product>(entity =>
        {
            entity.HasKey(p => p.Id);
            entity.Property(p => p.Name).HasMaxLength(200).IsRequired();
            entity.Property(p => p.Price).HasColumnType("decimal(18,2)");

            // Relationship: many products → one category
            entity.HasOne(p => p.Category)
                  .WithMany(c => c.Products)
                  .HasForeignKey(p => p.CategoryId)
                  .OnDelete(DeleteBehavior.Restrict);
        });

        // ─── Strategy 1: Model-based HasData seeding ─────────────────────────
        // Best for: static lookup / reference data (roles, status codes, categories)
        // Pros : migrations-aware, deterministic, zero extra code at startup
        // Cons : IDs must be hard-coded; large datasets inflate migration files
        modelBuilder.Entity<Category>().HasData(
            new Category { Id = 1, Slug = "electronics",  Name = "Electronics",  Description = "Gadgets and devices" },
            new Category { Id = 2, Slug = "books",        Name = "Books",        Description = "Printed and digital books" },
            new Category { Id = 3, Slug = "clothing",     Name = "Clothing",     Description = "Apparel and accessories" }
        );

        modelBuilder.Entity<Product>().HasData(
            new Product { Id = 1, Name = "Wireless Headphones",  Price = 79.99m,  Stock = 150, CategoryId = 1 },
            new Product { Id = 2, Name = "Mechanical Keyboard",  Price = 129.99m, Stock = 80,  CategoryId = 1 },
            new Product { Id = 3, Name = "Clean Code (Book)",    Price = 34.99m,  Stock = 200, CategoryId = 2 },
            new Product { Id = 4, Name = "Running Shoes",        Price = 89.99m,  Stock = 60,  CategoryId = 3 }
        );
    }
}
