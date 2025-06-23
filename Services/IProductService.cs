namespace EHSInventory.Services;

public interface IProductService
{
    Task<bool> ReorderProductAsync(long id, int newPosition);
    Task<long?> GetCategoryId(long id);
    Task<bool> FixOrderAsync(long id);
}