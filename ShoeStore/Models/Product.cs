namespace ShoeStore.Models;

/// <summary>Модель обуви в каталоге.</summary>
public class Product
{
    public int ProductId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? Composition { get; set; }
    public decimal Price { get; set; }
    public string? ImagePath { get; set; }
    public int CategoryId { get; set; }
    public int ManufacturerId { get; set; }

    public Category Category { get; set; } = null!;
    public Manufacturer Manufacturer { get; set; } = null!;
    public ICollection<ProductSize> ProductSizes { get; set; } = new List<ProductSize>();

    /// <summary>Количество пар суммарно по всем размерам.</summary>
    public int TotalQuantity => ProductSizes.Sum(ps => ps.Quantity);
}
