using EHSInventory.Models;
using Microsoft.EntityFrameworkCore;

namespace EHSInventory.Services;

public class OrderService
{
    private readonly InventoryDbContext _context;

    public OrderService(InventoryDbContext context)
    {
        _context = context;
    }

    public async Task<List<ShoppingCartItem>> ListOrderItems(long? orderId)
    {
        return await _context.ShoppingCartItems.Where(i => i.Order.OrderId == orderId).ToListAsync();
    }
}