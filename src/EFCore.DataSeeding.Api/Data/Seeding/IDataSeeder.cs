namespace EFCore.DataSeeding.Api.Data.Seeding;

/// <summary>
/// Marker interface for all runtime data seeders.
/// Register implementations with DI; they are called at app startup
/// only when the database is empty (idempotent guard included).
/// </summary>
public interface IDataSeeder
{
    /// <summary>
    /// Execution order — lower numbers run first.
    /// </summary>
    int Order { get; }

    Task SeedAsync(CancellationToken cancellationToken = default);
}
