using EFCore.DataSeeding.Api.Models;

namespace EFCore.DataSeeding.Api.Repositories;

/// <summary>
/// Abstraction over product data access.
/// Keeps controllers thin and facilitates unit-testing with mocks.
/// </summary>
public interface IProductRepository
{
    Task<IReadOnlyList<Product>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<Product?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Product>> GetByCategorySlugAsync(string slug, CancellationToken cancellationToken = default);
}
