namespace EHSInventory.Models;

public class ShoppingCartItem
{
    public long? ShoppingCartItemId { get; set; }
    public ShoppingCart? ShoppingCart { get; set; }
    public string Name { get; set; } = String.Empty;
    public ProductUnit Unit { get; set; }
    public int Count { get; set; }
}