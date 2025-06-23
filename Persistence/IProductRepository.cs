using EHSInventory.Models;

namespace EHSInventory.Persistence;

public interface IProductRepository
{
    Task<Product?> GetByIdAsync(long id);
    Task<List<Product>> GetAllOrderedAsync(long id);
    Task<long?> GetCategoryIdAsync(long? id);
    Task<int> SaveChangesAsync();
}