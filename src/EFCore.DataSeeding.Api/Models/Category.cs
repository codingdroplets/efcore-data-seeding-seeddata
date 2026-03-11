using System.Text.Json.Serialization;

namespace EFCore.DataSeeding.Api.Models;

/// <summary>
/// Represents a product category in the catalog.
/// </summary>
public class Category
{
    public int Id { get; set; }

    /// <summary>Unique slug used for URL routing (e.g. "electronics").</summary>
    public string Slug { get; set; } = string.Empty;

    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }

    // Navigation property — JsonIgnore prevents Product → Category → Products infinite cycle
    [JsonIgnore]
    public ICollection<Product> Products { get; set; } = new List<Product>();
}
