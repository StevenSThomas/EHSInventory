using EHSInventory.Models;

namespace EHSInventory.Services;

public interface ICatalogService
{
    Task<bool> AddProduct(string userName, string categoryName, string name, ProductUnit unit, int quantity, string? grangerNum, DateTime? expirationDate, string? description, string? photo);
    Task<bool> ReorderProductAsync(long id, int newPosition);
    Task<long?> GetCategoryId(long id);
    Task<bool> FixOrderAsync(long id);
}