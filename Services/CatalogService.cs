using EHSInventory.Models;
using EHSInventory.Persistence;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using Microsoft.EntityFrameworkCore.ChangeTracking;

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

    public async Task<List<ProductCategory>> ListCategories()
    {
        return await _context.ProductCategories.OrderBy(c => c.DisplayOrder).ToListAsync();
    }

    public async Task<List<Product>> ListProducts(long? catId)
    {
        return await _context.Products
        .Where(p => p.Category.ProductCategoryId == catId)
        .OrderBy(p => p.DisplayOrder)
        .ToListAsync();
    }

    public async Task<bool> AddCategory(string userName, ProductCategory category)
    {
        _context.Add(category);
        await _context.SaveChangesAsync();
        return true;
    }

        public async Task<bool> UpdateCategory(string userName, EditCategoryView category, string comment)
    {
        var originalCategory = await _context.ProductCategories.FindAsync(category.ProductCategoryId);

        if (originalCategory == null)
        {
            return false;
        }

        originalCategory.ProductCategoryId = category.ProductCategoryId;
        originalCategory.Name = category.Name;
        originalCategory.DisplayOrder = category.DisplayOrder;
        originalCategory.Icon = category.Icon;

        _context.Update(originalCategory);
        
        var modifiedEntries = _context.ChangeTracker.Entries().Where(e => e.State == EntityState.Modified);
        string changeJson = Compare(modifiedEntries);
        CategoryHistory history = new CategoryHistory
        {
            CreatedDt = DateTime.Now,
            CreatedBy = userName,
            CategoryId = category.ProductCategoryId,
            ChangeType = CategoryHistory.changeType.update,
            ChangeJson = changeJson,
            Comment = comment
        };
        _context.Add(history);
        
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> DeleteCategory(string userName, long id, string comment)
    {
        ProductCategory? category = await _context.ProductCategories.FindAsync(id);
        if (category == null)
        {
            return false;
        }

        _context.Remove(category);

        CategoryHistory history = new CategoryHistory
        {
            CreatedDt = DateTime.Now,
            CreatedBy = userName,
            CategoryId = category.ProductCategoryId,
            ChangeType = CategoryHistory.changeType.delete,
            Comment = comment
        };
        _context.Add(history);
        await _context.SaveChangesAsync();

        return true;
    }

    public async Task<bool> AddProduct(string userName, long id, Product product)
    {
        product.Category = await _context.ProductCategories.FindAsync(id);
        var products = await ListProducts(id);

        if (products.Any())
        {
            product.DisplayOrder = products.Max(p => p.DisplayOrder) + 1;
        }
        else
        {
            product.DisplayOrder = 1;
        }

        _context.Add(product);
        await _context.SaveChangesAsync();

        return true;
    }

        public bool AddProduct(string userName, string categoryName, string name, ProductUnit unit, int quantity, string? grangerNum, DateTime? expirationDate, string? description, string? photo)
    {
        ProductCategory? category = _context.ProductCategories.FirstOrDefault(c => categoryName.Equals(c.Name));

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
        _context.SaveChanges();

        return true;
    }

    public async Task<bool> UpdateProduct(string userName, EditProductView product, string comment)
    {
        var originalProduct = await _context.Products.FindAsync(product.ProductId);
        if (originalProduct == null)
        {
            return false;
        }

        originalProduct.Name = product.Name;
        originalProduct.Unit = product.Unit;
        originalProduct.GrangerNum = product.GrangerNum;
        originalProduct.Description = product.Description;
        originalProduct.Photo = product.Photo;
        originalProduct.ExpirationDate = product.ExpirationDate;

        _context.Update(originalProduct);

        var modifiedEntries = _context.ChangeTracker.Entries().Where(e => e.State == EntityState.Modified);
        string changeJson = Compare(modifiedEntries);

        ProductHistory history = new ProductHistory
        {
            CreatedDt = DateTime.Now,
            CreatedBy = userName,
            ProductId = product.ProductId,
            ChangeType = ProductHistory.changeType.update,
            ChangeJson = changeJson,
            Comment = comment
        };
        _context.Add(history);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> DeleteProduct(string userName, long id, string comment)
    {
        Product? product = await _context.Products.FindAsync(id);
        await FixOrderAsync(id);

        if (product == null)
        {
            return false;
        }

        _context.Remove(product);

        ProductHistory history = new ProductHistory
        {
            CreatedDt = DateTime.Now,
            CreatedBy = userName,
            ProductId = product.ProductId,
            ChangeType = ProductHistory.changeType.delete,
            Comment = comment
        };
        _context.Add(history);
        await _context.SaveChangesAsync();

        return true;
    }

    public async Task<bool> SetProductQuantity(string userName, long id, int newQuantity, string comment)
    {
        Product? product = await _context.Products.FindAsync(id);

        if (product == null)
        {
            return false;
        }

        string changeJson = CompareProducts(ProductHistory.changeType.setQuantity, product.Quantity, newQuantity);
        product.Quantity = newQuantity;
        _context.Update(product);

        ProductHistory history = new ProductHistory
        {
            CreatedDt = DateTime.Now,
            CreatedBy = userName,
            ProductId = id,
            ChangeType = ProductHistory.changeType.setQuantity,
            ChangeJson = changeJson,
            Comment = comment
        };

        _context.Add(history);
        await _context.SaveChangesAsync();

        return true;
    }

    public async Task<bool> SetProductDisplayOrder(string userName, long id, int newDisplayOrder)
    {
        Product? product = await _context.Products.FindAsync(id);

        if (product == null)
        {
            return false;
        }

        string changeJson = CompareProducts(ProductHistory.changeType.setDisplayOrder, product.DisplayOrder, newDisplayOrder);
        await ReorderProductAsync(id, newDisplayOrder);

        ProductHistory history = new ProductHistory
        {
            CreatedDt = DateTime.Now,
            CreatedBy = userName,
            ProductId = id,
            ChangeType = ProductHistory.changeType.setDisplayOrder,
            ChangeJson = changeJson
        };
        _context.Add(history);
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

    public static string Compare(IEnumerable<EntityEntry> modifiedEntries)
    {
        var entry = modifiedEntries.First();
        List<Change> changes = new List<Change>();

        foreach (var property in entry.Properties)
        {
            if (property.IsModified)
            {
                var change = new Change
                {
                    FieldChanged = property.Metadata.Name ?? String.Empty,
                    Before = property.OriginalValue?.ToString() ?? String.Empty,
                    After = property.CurrentValue?.ToString() ?? String.Empty
                };
                changes.Add(change);
            }
        }

        return JsonSerializer.Serialize(changes);
    }

    public static string CompareProducts(ProductHistory.changeType changeType, int before, int after)
    {
        List<Change> changes = new List<Change>();
        Change change = new Change
        {
            FieldChanged = changeType.ToString(),
            Before = before.ToString(),
            After = after.ToString()
        };
        changes.Add(change);

        return JsonSerializer.Serialize(changes);
    }
}