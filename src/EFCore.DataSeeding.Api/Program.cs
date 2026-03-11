using EFCore.DataSeeding.Api.Data;
using EFCore.DataSeeding.Api.Data.Seeding;
using EFCore.DataSeeding.Api.Repositories;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// ── Database ─────────────────────────────────────────────────────────────────
// Switch to UseSqlServer(...) for production; InMemory is used here so
// the sample builds and runs without a live SQL Server instance.
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseInMemoryDatabase("CodingDropletsSeedDemo"));

// ── Repository pattern ────────────────────────────────────────────────────────
builder.Services.AddScoped<IProductRepository, ProductRepository>();

// ── Data seeders (Strategy 2: runtime custom seeders) ─────────────────────────
// Register each seeder + the orchestrator
builder.Services.AddScoped<IDataSeeder, ProductSeeder>();
builder.Services.AddScoped<DatabaseSeeder>();

// ── API / Swagger ─────────────────────────────────────────────────────────────
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        // Prevent infinite cycles from bidirectional navigation properties
        // (e.g. Product → Category → Products → Product → ...)
        options.JsonSerializerOptions.ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles;
        options.JsonSerializerOptions.DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull;
    });
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new()
    {
        Title = "EF Core Data Seeding Demo",
        Version = "v1",
        Description = "Demonstrates Model-based HasData seeding and custom runtime seeders in EF Core."
    });
    // Include XML comments for controller docs
    var xmlFile = $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    if (File.Exists(xmlPath))
        c.IncludeXmlComments(xmlPath);
});

var app = builder.Build();

// ── Apply migrations + run seeders at startup ─────────────────────────────────
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

    // Strategy 1 (HasData) is applied automatically via EnsureCreated / Migrate.
    // For InMemory we use EnsureCreated; for SQL Server switch to db.Database.Migrate().
    await db.Database.EnsureCreatedAsync();

    // Strategy 2: run custom runtime seeders
    var seeder = scope.ServiceProvider.GetRequiredService<DatabaseSeeder>();
    await seeder.SeedAllAsync();
}

// ── Middleware ────────────────────────────────────────────────────────────────
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();

// Make the implicit Program class accessible to integration tests
public partial class Program { }
