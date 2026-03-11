# EF Core Data Seeding: Production-Ready Patterns for .NET

> Practical code samples for **Advanced Data Seeding Strategies in Entity Framework Core** — covering both model-based `HasData` seeding and custom runtime seeders with full idempotency guards.

[![.NET](https://img.shields.io/badge/.NET-10.0-512BD4?logo=dotnet)](https://dotnet.microsoft.com)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](LICENSE)

---

## 🚀 What You'll Learn

This repository demonstrates two production-ready EF Core data seeding strategies:

| Strategy | Best For | Pros | Cons |
|---|---|---|---|
| **Model-based `HasData`** | Static reference data (lookup tables, roles, categories) | Migrations-aware, zero startup overhead, deterministic | Hard-coded IDs required; large datasets inflate migration files |
| **Custom Runtime Seeders** | Dynamic/computed data, large datasets, external source data | Full C# expressiveness, no migration bloat, DB-generated IDs | Runs at startup, must guard against duplicates |

---

## 📁 Project Structure

```
efcore-data-seeding-seeddata/
├── src/
│   └── EFCore.DataSeeding.Api/
│       ├── Controllers/
│       │   └── ProductsController.cs        # REST endpoints to verify seeded data
│       ├── Data/
│       │   ├── AppDbContext.cs               # DbContext with HasData configuration
│       │   └── Seeding/
│       │       ├── IDataSeeder.cs            # Seeder marker interface
│       │       ├── DatabaseSeeder.cs         # Orchestrates all seeders in order
│       │       └── ProductSeeder.cs          # Custom runtime seeder (Strategy 2)
│       ├── Models/
│       │   ├── Category.cs
│       │   └── Product.cs
│       ├── Repositories/
│       │   ├── IProductRepository.cs
│       │   └── ProductRepository.cs         # Repository pattern with AsNoTracking
│       └── Program.cs                       # DI wiring + startup seeding
└── tests/
    └── EFCore.DataSeeding.Tests/
        ├── HasDataSeedingTests.cs            # Tests for Strategy 1
        └── ProductSeederTests.cs            # Tests for Strategy 2 (idempotency verified)
```

---

## ⚡ Quick Start

### Prerequisites
- [.NET 10 SDK](https://dotnet.microsoft.com/download) (or .NET 8/9 — update `TargetFramework` in `.csproj`)

### Run Locally (In-Memory Database)

```bash
git clone https://github.com/codingdroplets/efcore-data-seeding-seeddata.git
cd efcore-data-seeding-seeddata

dotnet run --project src/EFCore.DataSeeding.Api
```

Open Swagger UI: **https://localhost:{port}/swagger**

### Run Tests

```bash
dotnet test -c Release
```

---

## 🗄️ Strategy 1: Model-Based `HasData` Seeding

Configured inside `OnModelCreating` in `AppDbContext.cs`. EF Core applies this data through migrations (or `EnsureCreated` for InMemory).

```csharp
modelBuilder.Entity<Category>().HasData(
    new Category { Id = 1, Slug = "electronics", Name = "Electronics" },
    new Category { Id = 2, Slug = "books",        Name = "Books" },
    new Category { Id = 3, Slug = "clothing",     Name = "Clothing" }
);
```

> ⚠️ **Important:** `HasData` requires hard-coded primary keys. Shadow properties and navigation properties are not supported — use flat scalar values only.

---

## ⚙️ Strategy 2: Custom Runtime Seeder

`ProductSeeder` implements `IDataSeeder` and is called at startup by `DatabaseSeeder`. It includes an **idempotency guard** so it is safe to run on every startup.

```csharp
public async Task SeedAsync(CancellationToken cancellationToken = default)
{
    // Guard: skip if already seeded
    bool alreadySeeded = await _db.Products
        .AnyAsync(p => p.Stock > 500, cancellationToken);

    if (alreadySeeded) return;

    // Fetch FK by slug — no hard-coded IDs
    var electronics = await _db.Categories
        .FirstAsync(c => c.Slug == "electronics", cancellationToken);

    await _db.Products.AddRangeAsync(new List<Product>
    {
        new() { Name = "USB-C Hub (7-in-1)", Price = 49.99m, Stock = 750, CategoryId = electronics.Id },
        // ...
    }, cancellationToken);

    await _db.SaveChangesAsync(cancellationToken);
}
```

### Registering Seeders

In `Program.cs`:

```csharp
builder.Services.AddScoped<IDataSeeder, ProductSeeder>();
builder.Services.AddScoped<DatabaseSeeder>();
```

Add more seeders by implementing `IDataSeeder` and registering them. Control execution order via the `Order` property.

---

## 🔄 Switching to SQL Server

The sample uses InMemory for zero-friction local runs. To use SQL Server:

1. Update `Program.cs`:
   ```csharp
   builder.Services.AddDbContext<AppDbContext>(options =>
       options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));
   ```

2. Replace `EnsureCreatedAsync()` with:
   ```csharp
   await db.Database.MigrateAsync();
   ```

3. Add your connection string to `appsettings.json`.

---

## 🧪 Test Coverage

| Test | Strategy |
|---|---|
| `HasData_Seeds_ThreeCategories` | Strategy 1 |
| `HasData_Seeds_ElectronicsCategory_WithCorrectSlug` | Strategy 1 |
| `HasData_Seeds_FourBaseProducts` | Strategy 1 |
| `HasData_Products_HaveCorrectCategoryAssignments` | Strategy 1 |
| `ProductSeeder_InsertsHighStockProducts` | Strategy 2 |
| `ProductSeeder_IsIdempotent_DoesNotDuplicate` | Strategy 2 |
| `ProductSeeder_AssignsCorrectCategories` | Strategy 2 |

---

## 📚 Further Reading

- [EF Core — Data Seeding (Microsoft Docs)](https://learn.microsoft.com/en-us/ef/core/modeling/data-seeding)
- [EF Core — Migrations Overview](https://learn.microsoft.com/en-us/ef/core/managing-schemas/migrations/)

---

Visit Now: https://codingdroplets.com  
Join our Patreon to Learn & Level Up: https://www.patreon.com/CodingDroplets

---

## 📄 License

MIT © [Coding Droplets](https://codingdroplets.com)
