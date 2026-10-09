namespace ShoeStore.Models;

public class Order
{
    public int OrderId { get; set; }
    public DateTime OrderDate { get; set; }
    public int UserId { get; set; }

    public User User { get; set; } = null!;
    public ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();

    public decimal TotalSum => OrderItems.Sum(i => i.UnitPrice * i.Quantity);
}
