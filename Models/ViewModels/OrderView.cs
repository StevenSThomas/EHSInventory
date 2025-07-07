namespace EHSInventory.Models;

public class OrderView : Order
{
    public List<ShoppingCartItem> Items { get; set; } = new List<ShoppingCartItem>();
}