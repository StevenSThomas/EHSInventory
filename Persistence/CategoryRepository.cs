using EHSInventory.Models;
using Microsoft.EntityFrameworkCore;

namespace EHSInventory.Persistence;

public class CategoryRepository : ICategoryRepository
{
    private readonly InventoryDbContext _context;

    public CategoryRepository(InventoryDbContext context)
    {
        _context = context;
    }

    public async Task<ProductCategory?> GetByIdAsync(long id)
    {
        return await _context.ProductCategories.FindAsync(id);
    }

    public async Task<List<Product>> GetProductsOrderedAsync(long? id)
    {
        return await _context.Products.Where(p =>
        p.Category != null && p.Category.ProductCategoryId == id)
        .OrderBy(p => p.DisplayOrder)
        .ToListAsync();
    }

    public async Task<int> SaveChangesAsync()
    {
        return await _context.SaveChangesAsync();
    }
}