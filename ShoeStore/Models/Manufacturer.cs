namespace ShoeStore.Models;

/// <summary>Производство (страна или фабрика-изготовитель).</summary>
public class Manufacturer
{
    public int ManufacturerId { get; set; }
    public string Name { get; set; } = string.Empty;

    public ICollection<Product> Products { get; set; } = new List<Product>();
}
