namespace EHSInventory.Models;

public class Order
{
    public enum OrderStatus
    {
        pending,
        readyForPickup,
        backOrdered,
        closed,
        canceled
    }

    public long? OrderId { get; set; }
    public DateTime CreatedDt { get; set; }
    public OrderStatus Status { get; set; }
    public string Requester { get; set; } = String.Empty;
}