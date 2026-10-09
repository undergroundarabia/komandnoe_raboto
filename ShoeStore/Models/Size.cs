namespace ShoeStore.Models;

public class Size
{
    public int SizeId { get; set; }
    public decimal Value { get; set; }

    public ICollection<ProductSize> ProductSizes { get; set; } = new List<ProductSize>();
}
