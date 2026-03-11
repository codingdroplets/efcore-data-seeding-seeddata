namespace EFCore.DataSeeding.Api.Data.Seeding;

/// <summary>
/// Orchestrates all registered <see cref="IDataSeeder"/> implementations
/// in <see cref="IDataSeeder.Order"/> sequence.
///
/// Called once at application startup (see Program.cs).
/// </summary>
public class DatabaseSeeder
{
    private readonly IEnumerable<IDataSeeder> _seeders;
    private readonly ILogger<DatabaseSeeder> _logger;

    public DatabaseSeeder(IEnumerable<IDataSeeder> seeders, ILogger<DatabaseSeeder> logger)
    {
        _seeders = seeders;
        _logger = logger;
    }

    public async Task SeedAllAsync(CancellationToken cancellationToken = default)
    {
        foreach (var seeder in _seeders.OrderBy(s => s.Order))
        {
            _logger.LogInformation("Running seeder: {Seeder}", seeder.GetType().Name);
            await seeder.SeedAsync(cancellationToken);
        }
    }
}
