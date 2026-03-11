using EFCore.DataSeeding.Api.Repositories;
using Microsoft.AspNetCore.Mvc;

namespace EFCore.DataSeeding.Api.Controllers;

/// <summary>
/// Exposes product catalog endpoints.
/// Demonstrates that seeded data is immediately queryable via the API.
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class ProductsController : ControllerBase
{
    private readonly IProductRepository _repo;

    public ProductsController(IProductRepository repo) => _repo = repo;

    /// <summary>Returns all products.</summary>
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
    {
        var products = await _repo.GetAllAsync(cancellationToken);
        return Ok(products);
    }

    /// <summary>Returns a single product by ID.</summary>
    [HttpGet("{id:int}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(int id, CancellationToken cancellationToken)
    {
        var product = await _repo.GetByIdAsync(id, cancellationToken);
        return product is null ? NotFound() : Ok(product);
    }

    /// <summary>Returns all active products for a given category slug.</summary>
    [HttpGet("by-category/{slug}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetByCategory(string slug, CancellationToken cancellationToken)
    {
        var products = await _repo.GetByCategorySlugAsync(slug, cancellationToken);
        return Ok(products);
    }
}
