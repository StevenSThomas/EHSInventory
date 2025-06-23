using EHSInventory.Models;
using EHSInventory.Persistence;

namespace EHSInventory.Services;

public class ProductService : IProductService
{
    private readonly IProductRepository _productRepository;
    private readonly ICategoryRepository _categoryRepository;
    private readonly ILogger<ProductService> _logger;

    public ProductService(IProductRepository productRepository, ICategoryRepository categoryRepository, ILogger<ProductService> logger)
    {
        _productRepository = productRepository;
        _categoryRepository = categoryRepository;
        _logger = logger;
    }

    public async Task<bool> ReorderProductAsync(long id, int newPosition)
    {
        var product = await _productRepository.GetByIdAsync(id);

        if (product == null)
        {
            return false;
        }

        var categoryId = await _productRepository.GetCategoryIdAsync(id);
        var orderedProductList = await _categoryRepository.GetProductsOrderedAsync(categoryId);

        if (newPosition < 1 || newPosition > orderedProductList.Count)
        {
            return false;
        }
        product.DisplayOrder = newPosition;
        orderedProductList.Remove(product);
        orderedProductList.Insert(newPosition-1, product);

        for (int i = 1; i <= orderedProductList.Count; i++)
        {
            Console.WriteLine(orderedProductList[i - 1].ProductId);
            orderedProductList[i - 1].DisplayOrder = i;
        }

        await _productRepository.SaveChangesAsync();
        return true;

    }

    public async Task<long?> GetCategoryId(long id)
    {
        return await _productRepository.GetCategoryIdAsync(id);
    }

    public async Task<bool> FixOrderAsync(long id)
    {
        var categoryId = await _productRepository.GetCategoryIdAsync(id);
        var products = await _categoryRepository.GetProductsOrderedAsync(categoryId);


        var displayOrder = 1;
        foreach (Product p in products)
        {
            if (p.ProductId != id)
            {
                p.DisplayOrder = displayOrder;
                displayOrder++;
            }
        }

        await _productRepository.SaveChangesAsync();

        return true;
    }
}