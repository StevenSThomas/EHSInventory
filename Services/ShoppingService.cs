using EHSInventory.Models;
using Microsoft.EntityFrameworkCore;

namespace EHSInventory.Services;

public class ShoppingService
{
    private InventoryDbContext _context;

    public ShoppingService(InventoryDbContext context)
    {
        _context = context;
    }

    public async Task<List<Product>> ListProducts(long? catId)
    {
        var products = await _context.Products
            .Where(p => p.Category.ProductCategoryId == catId)
            .GroupBy(p => new { p.Name, p.Unit })
            .Select(g => g.OrderBy(p => p.ProductId).First())
            // .OrderBy(p => p.DisplayOrder)
            .ToListAsync();

        return products.OrderBy(p => p.DisplayOrder).ToList(); // note: OrderBy is done in memory, might only work for small numbers of products
    }

    public async Task<List<ShoppingCartItem>> ListShoppingCartItems(string userName)
    {
        return await _context.ShoppingCartItems
            .Where(i => i.ShoppingCart.Username != null && i.ShoppingCart.Username.Equals(userName))
            .ToListAsync();
    }

    public async Task<bool> AddToCart(string userName, string productName, ProductUnit unit, int count)
    {
        if (!_context.Products.Any(p => p.Name.Equals(productName) && p.Unit == unit))
        {
            return false;
        }

        var shoppingCart = await _context.ShoppingCarts.FirstOrDefaultAsync(c => c.Username.Equals(userName));
        ShoppingCartItem? item;

        if (shoppingCart != null)
        {
            item = await _context.ShoppingCartItems.FirstOrDefaultAsync(
                i => i.ShoppingCart.ShoppingCartId == shoppingCart.ShoppingCartId
                && i.Name.Equals(productName)
                && i.Unit == unit);

            if (item != null)
            {
                item.Count = count;
                _context.Update(item);
                await _context.SaveChangesAsync();
                return true;
            }
        }
        else
        {
            shoppingCart = new ShoppingCart { Username = userName };
            _context.Add(shoppingCart);
            await _context.SaveChangesAsync();
        }

        item = new ShoppingCartItem
        {
            Name = productName,
            Unit = unit,
            ShoppingCart = shoppingCart,
            Count = count
        };
        _context.Add(item);
        await _context.SaveChangesAsync();

        return true;
    }

    public async Task<bool> RemoveFromCart(string userName, string productName, ProductUnit unit)
    {
        var item = _context.ShoppingCartItems.FirstOrDefaultAsync(p =>
            p.ShoppingCart.Username.Equals(userName) &&
            p.Name.Equals(productName) &&
            p.Unit == unit
        );

        if (item == null)
        {
            return false;
        }

        _context.Remove(item);
        await _context.SaveChangesAsync();

        return true;
    }

    /*
    public async Task<bool> PlaceOrder(string userName)
    {
        var shoppingCart = await _context.ShoppingCarts.FirstOrDefaultAsync(c => c.Username.Equals(userName));

        if (shoppingCart == null)
        {
            return false;
        }

        var items = await _context.ShoppingCartItems.Where(i => i.ShoppingCart.ShoppingCartId == shoppingCart.ShoppingCartId).ToListAsync();
        foreach (var item in items)
        {
            var product = await _context.Products.FindAsync(item.ProductId);
            if (product == null)
            {
                return false;
            }

            product.Quantity -= item.Count;
            if (product.Quantity < 0)
            {
                product.Quantity = 0;
            }
            _context.Update(product);
            _context.Remove(item);
            await _context.SaveChangesAsync();
        }

        _context.Remove(shoppingCart);
        await _context.SaveChangesAsync();

        return true;
    }
    */
}