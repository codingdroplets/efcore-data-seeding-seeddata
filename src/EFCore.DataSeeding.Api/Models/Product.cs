namespace EFCore.DataSeeding.Api.Models;

/// <summary>
/// Represents a product in the catalog.
/// </summary>
public class Product
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public decimal Price { get; set; }
    public int Stock { get; set; }
    public bool IsActive { get; set; } = true;

    // Foreign key
    public int CategoryId { get; set; }

    // Navigation property
    public Category Category { get; set; } = null!;
}
