using EHSInventory.Models;

namespace EHSInventory.Services;

public interface ICatalogService
{
    Task<List<ProductCategory>> ListCategories();
    Task<List<Product>> ListProducts(long? catId);
    Task<bool> AddCategory(string userName, ProductCategory category);
    Task<bool> UpdateCategory(string userName, EditCategoryView category, string comment);
    Task<bool> DeleteCategory(string userName, long id, string comment);
    Task<bool> AddProduct(string userName, long id, EditProductView product);
    bool AddProduct(string userName, string categoryName, string name, ProductUnit unit, int quantity, string? grangerNum, DateTime? expirationDate, string? description, string? photo);
    Task<bool> UpdateProduct(string userName, EditProductView product, string comment);
    Task<bool> DeleteProduct(string userName, long id, string comment);
    Task<bool> SetProductQuantity(string userName, long id, int newQuantity, string comment);
    Task<bool> SetProductDisplayOrder(string userName, long id, int newDisplayOrder);
    Task<bool> ReorderProductAsync(long id, int newPosition);
    Task<long?> GetCategoryId(long id);
    Task<List<ProductHistory>> GetProductHistories(long productId);
    Task<bool> FixOrderAsync(long id);
}