namespace ShoeStore.Models;

public class OrderItem
{
    public int OrderItemId { get; set; }
    public int OrderId { get; set; }
    public int ProductSizeId { get; set; }
    public int Quantity { get; set; }

    /// <summary>Цена за пару со скидкой на момент оформления заказа.</summary>
    public decimal UnitPrice { get; set; }

    public Order Order { get; set; } = null!;
    public ProductSize ProductSize { get; set; } = null!;
}
