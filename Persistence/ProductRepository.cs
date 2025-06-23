using EHSInventory.Models;
using Microsoft.EntityFrameworkCore;

namespace EHSInventory.Persistence;

public class ProductRepository : IProductRepository
{
    private readonly InventoryDbContext _context;

    public ProductRepository(InventoryDbContext context)
    {
        _context = context;
    }

    public async Task<Product?> GetByIdAsync(long id)
    {
        return await _context.Products.FindAsync(id);
    }

    public async Task<List<Product>> GetAllOrderedAsync(long id)
    {
        var product = await _context.Products.FindAsync(id);
        return await _context.Products.OrderBy(p => p.DisplayOrder).Where(
            p => product != null
            && p.DisplayOrder == product.DisplayOrder)
            .ToListAsync();
    }

    public async Task<long?> GetCategoryIdAsync(long? id)
    {
        var product = await _context.Products.Include(p => p.Category).FirstOrDefaultAsync(p => p.ProductId == id);
        Console.WriteLine(product?.Category?.ProductCategoryId);
        return product?.Category?.ProductCategoryId;
    }

    public async Task<int> SaveChangesAsync()
    {
        return await _context.SaveChangesAsync();
    }
}