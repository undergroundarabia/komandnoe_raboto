namespace ShoeStore.Models;

/// <summary>Товарная позиция: модель + размер + доступное количество.</summary>
public class ProductSize
{
    public int ProductSizeId { get; set; }
    public int ProductId { get; set; }
    public int SizeId { get; set; }
    public int Quantity { get; set; }

    public Product Product { get; set; } = null!;
    public Size Size { get; set; } = null!;
    public ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
}
