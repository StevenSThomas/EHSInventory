using EHSInventory.Models;

namespace EHSInventory.Persistence;

public interface ICategoryRepository
{
    Task<ProductCategory?> GetByIdAsync(long id);
    Task<List<Product>> GetProductsOrderedAsync(long? id);
    Task<int> SaveChangesAsync();
}