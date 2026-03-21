# EF Core Data Seeding: Production-Ready Patterns for .NET

> **Two battle-tested seeding strategies in one runnable sample** — Model-based `HasData` for static reference data and custom runtime seeders with full idempotency guards for dynamic datasets.

[![.NET](https://img.shields.io/badge/.NET-10.0-512BD4?logo=dotnet)](https://dotnet.microsoft.com/)
[![EF Core](https://img.shields.io/badge/EF%20Core-10.0-512BD4?logo=dotnet)](https://learn.microsoft.com/en-us/ef/core/)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](LICENSE)
[![Visit CodingDroplets](https://img.shields.io/badge/Website-codingdroplets.com-blue?style=flat&logo=google-chrome&logoColor=white)](https://codingdroplets.com/)
[![YouTube](https://img.shields.io/badge/YouTube-CodingDroplets-red?style=flat&logo=youtube&logoColor=white)](https://www.youtube.com/@CodingDroplets)
[![Patreon](https://img.shields.io/badge/Patreon-Support%20Us-orange?style=flat&logo=patreon&logoColor=white)](https://www.patreon.com/CodingDroplets)
[![Buy Me a Coffee](https://img.shields.io/badge/Buy%20Me%20a%20Coffee-Support%20Us-yellow?style=flat&logo=buy-me-a-coffee&logoColor=black)](https://buymeacoffee.com/codingdroplets)
[![GitHub](https://img.shields.io/badge/GitHub-codingdroplets-black?style=flat&logo=github&logoColor=white)](http://github.com/codingdroplets/)

---

## 🚀 Support the Channel — Join on Patreon

If this sample saved you time, consider joining our Patreon community.
You'll get **exclusive .NET tutorials, premium code samples, and early access** to new content — all for the price of a coffee.

👉 **[Join CodingDroplets on Patreon](https://www.patreon.com/CodingDroplets)**

Prefer a one-time tip? [Buy us a coffee ☕](https://buymeacoffee.com/codingdroplets)

---

## 🎯 What You'll Learn

- How to use **model-based `HasData`** seeding for static reference data (roles, categories, lookup tables)
- How to build **custom runtime seeders** with `IDataSeeder` for dynamic or large datasets
- How to write **idempotency guards** so seeders are safe to run on every startup
- How to **orchestrate multiple seeders** in a controlled order with `DatabaseSeeder`
- How to switch from **InMemory to SQL Server** with a two-line change

---

## 📋 Seeding Strategies Compared

| Strategy | Best For | Pros | Cons |
|---|---|---|---|
| **Model-based `HasData`** | Static reference data (lookup tables, roles, categories) | Migrations-aware, zero startup overhead, deterministic | Hard-coded IDs required; large datasets inflate migration files |
| **Custom Runtime Seeders** | Dynamic/computed data, large datasets, external source data | Full C# expressiveness, no migration bloat, DB-generated IDs | Runs at startup; must guard against duplicates |

---

## 🗺️ Architecture Overview

```
App Startup (Program.cs)
        │
        ▼
┌───────────────────────────────────────────────────┐
│               DatabaseSeeder                      │
│  Orchestrates all IDataSeeder implementations     │
│  in order (Order property)                        │
│                                                   │
│  ┌──────────────────────────────────────────────┐ │
│  │  Strategy 1: AppDbContext.OnModelCreating    │ │
│  │  modelBuilder.Entity<T>().HasData(...)       │ │
│  │  → Applied by EF migrations / EnsureCreated  │ │
│  └──────────────────────────────────────────────┘ │
│                                                   │
│  ┌──────────────────────────────────────────────┐ │
│  │  Strategy 2: ProductSeeder : IDataSeeder     │ │
│  │  → Guard: AnyAsync() check before inserting  │ │
│  │  → Resolves FK by slug (no hard-coded IDs)   │ │
│  │  → SaveChangesAsync()                        │ │
│  └──────────────────────────────────────────────┘ │
└───────────────────────────────────────────────────┘
        │
        ▼
  Database ready with seeded reference + product data
```

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

## 🛠️ Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0) (or .NET 8/9 — update `TargetFramework` in `.csproj`)
- Any IDE: Visual Studio 2022+, VS Code, or JetBrains Rider

---

## ⚡ Quick Start

```bash
# Clone the repo
git clone https://github.com/codingdroplets/efcore-data-seeding-seeddata.git
cd efcore-data-seeding-seeddata

# Run (uses InMemory database — no SQL Server setup needed)
dotnet run --project src/EFCore.DataSeeding.Api

# Open Swagger UI → https://localhost:{port}/swagger
```

---

## 🔧 How It Works

### Strategy 1 — Model-Based `HasData` (Static Reference Data)

Configured inside `OnModelCreating` in `AppDbContext.cs`. Applied automatically by EF Core on `EnsureCreated` or migration:

```csharp
modelBuilder.Entity<Category>().HasData(
    new Category { Id = 1, Slug = "electronics", Name = "Electronics" },
    new Category { Id = 2, Slug = "books",        Name = "Books" },
    new Category { Id = 3, Slug = "clothing",     Name = "Clothing" }
);
```

> ⚠️ **Important:** `HasData` requires hard-coded primary keys. Navigation properties are not supported — use flat scalar values only.

### Strategy 2 — Custom Runtime Seeder (Dynamic Data)

`ProductSeeder` implements `IDataSeeder` and is called at startup. It uses an **idempotency guard** so it is safe to run on every startup:

```csharp
public async Task SeedAsync(CancellationToken cancellationToken = default)
{
    // Guard: skip if already seeded
    bool alreadySeeded = await _db.Products
        .AnyAsync(p => p.Stock > 500, cancellationToken);

    if (alreadySeeded) return;

    // Resolve FK by slug — no hard-coded IDs
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

### Register Seeders in Program.cs

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
2. Replace `EnsureCreatedAsync()` with `MigrateAsync()`.
3. Add your connection string to `appsettings.json`.

---

## 🧪 Running Tests

```bash
dotnet test -c Release
```

| Test | Strategy Covered |
|---|---|
| `HasData_Seeds_ThreeCategories` | Strategy 1 |
| `HasData_Seeds_ElectronicsCategory_WithCorrectSlug` | Strategy 1 |
| `HasData_Seeds_FourBaseProducts` | Strategy 1 |
| `HasData_Products_HaveCorrectCategoryAssignments` | Strategy 1 |
| `ProductSeeder_InsertsHighStockProducts` | Strategy 2 |
| `ProductSeeder_IsIdempotent_DoesNotDuplicate` | Strategy 2 |
| `ProductSeeder_AssignsCorrectCategories` | Strategy 2 |

---

## 📚 References

- [EF Core — Data Seeding (Microsoft Docs)](https://learn.microsoft.com/en-us/ef/core/modeling/data-seeding)
- [EF Core — Migrations Overview](https://learn.microsoft.com/en-us/ef/core/managing-schemas/migrations/)
- [EF Core — InMemory Database Provider](https://learn.microsoft.com/en-us/ef/core/providers/in-memory/)

---

## 📄 License

This project is licensed under the [MIT License](LICENSE).

---

## 🔗 Connect with CodingDroplets

| Platform | Link |
|----------|------|
| 🌐 Website | https://codingdroplets.com/ |
| 📺 YouTube | https://www.youtube.com/@CodingDroplets |
| 🎁 Patreon | https://www.patreon.com/CodingDroplets |
| ☕ Buy Me a Coffee | https://buymeacoffee.com/codingdroplets |
| 💻 GitHub | http://github.com/codingdroplets/ |

> **Want more samples like this?** [Support us on Patreon](https://www.patreon.com/CodingDroplets) or [buy us a coffee ☕](https://buymeacoffee.com/codingdroplets) — every bit helps keep the content coming!
