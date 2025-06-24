using EHSInventory.Models;
using EHSInventory.Persistence;
using Microsoft.EntityFrameworkCore;

namespace EHSInventory.Services;

public class CatalogService : ICatalogService
{
    private readonly IProductRepository _productRepository;
    private readonly ICategoryRepository _categoryRepository;
    private readonly InventoryDbContext _context;
    private readonly ILogger<CatalogService> _logger;

    public CatalogService(IProductRepository productRepository, ICategoryRepository categoryRepository, InventoryDbContext context, ILogger<CatalogService> logger)
    {
        _productRepository = productRepository;
        _categoryRepository = categoryRepository;
        _context = context;
        _logger = logger;
    }

    public async Task<bool> AddProduct(string userName, string categoryName, string name, ProductUnit unit, int quantity, string? grangerNum, DateTime? expirationDate, string? description, string? photo)
    {
        ProductCategory? category = await _context.ProductCategories.FirstOrDefaultAsync(c => categoryName.Equals(c.Name));

        if (category == null)
        {
            return false;
        }

        List<Product> products = _context.Products.OrderBy(p => p.DisplayOrder)
        .Where(p => p.Category != null && p.Category.ProductCategoryId == category.ProductCategoryId)
        .ToList();

        int displayOrder = products.Max(p => p.DisplayOrder) + 1;

        Product product = new Product
        {
            Category = category,
            Name = name,
            Unit = unit,
            Quantity = quantity,
            GrangerNum = grangerNum,
            ExpirationDate = expirationDate,
            DisplayOrder = displayOrder,
            Description = description,
            Photo = photo
        };

        _context.Add(product);
        await _context.SaveChangesAsync();

        return true;
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
        orderedProductList.Insert(newPosition - 1, product);

        for (int i = 1; i <= orderedProductList.Count; i++)
        {
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