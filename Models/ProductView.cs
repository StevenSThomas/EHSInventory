namespace EHSInventory.Models;

public class ProductView : Product
{
    public List<ProductHistory> ProductHistories { get; set; } = new List<ProductHistory>();
    public long? CategoryId { get; set; }
}