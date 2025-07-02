using EHSInventory.Models;
using EHSInventory.Persistence;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using System.Reflection;

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

        /*
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
        */

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

    public async Task<bool> AddProduct(string userName, long id, EditProductView productView)
    {
        Product product = new Product
        {
            Name = productView.Name,
            Unit = productView.Unit,
            GrangerNum = productView.GrangerNum,
            Description = productView.Description,
            Photo = productView.Photo,
            ExpirationDate = productView.ParsedDate ?? null
        };

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

        AddInitialHistory(userName, product);

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

        int displayOrder = products.Any() ? products.Max(p => p.DisplayOrder) + 1 : 1;

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

        AddInitialHistory(userName, product);

        return true;
    }

    public async Task<bool> UpdateProduct(string userName, EditProductView product, string comment)
    {
        var originalProduct = await _context.Products.FindAsync(product.ProductId);
        if (originalProduct == null)
        {
            return false;
        }
        string changeJson = Compare(originalProduct, product);

        originalProduct.Name = product.Name;
        // originalProduct.Quantity = product.Quantity;
        originalProduct.Unit = product.Unit;
        originalProduct.GrangerNum = product.GrangerNum;
        originalProduct.Description = product.Description;
        originalProduct.Photo = product.Photo;
        originalProduct.ExpirationDate = product.ParsedDate;

        _context.Update(originalProduct);

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

    /*
    public async Task<bool> RevertChange(long historyId)
    {
        var history = await _context.ProductHistories.FindAsync(historyId);
        if (history == null)
        {
            return false;
        }

        List<Change> changes = JsonSerializer.Deserialize<List<Change>>(history.ChangeJson);
        var product = await _context.Products.FindAsync(history.ProductId);
        if (changes == null) return false;
        if (product == null) return false;

        foreach (Change change in changes)
        {
            if (change.FieldChanged.Equals("Unit"))
            {
                ProductUnit unit = (ProductUnit)Enum.Parse(typeof(ProductUnit), (string)change.After);
                product.Unit = unit;
            }
            else if (change.FieldChanged.Equals("ExpirationDate"))
            {
                DateTime date = DateTime.Parse((string)change.After);
                product.ExpirationDate = date;
            }
            else
            {
                PropertyInfo field = product?.GetType().GetProperty(change.FieldChanged);
                field.SetValue(product, change.After);
            }
        }
    }
    */

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

    public async Task<List<ProductHistory>> GetProductHistories(long productId)
    {
        var productHistories = await _context.ProductHistories.Where(h => h.ProductId == productId).ToListAsync();

        foreach (var history in productHistories)
        {
            List<Change>? changes = JsonSerializer.Deserialize<List<Change>>(history.ChangeJson);
            history.Changes = changes;
        }

        return productHistories;
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

    public static string Compare(Product originalProduct, EditProductView product)
    {
        List<Change> changes = new List<Change>();

        if (!string.Equals(originalProduct.Name, product.Name))
        {
            changes.Add(new Change
            {
                FieldChanged = "Name",
                Before = originalProduct.Name,
                After = product.Name
            });
        }
        if (originalProduct.Unit != product.Unit)
        {
            changes.Add(new Change
            {
                FieldChanged = "Unit",
                Before = originalProduct.Unit.ToString(),
                After = product.Unit.ToString()
            });
        }
        if (!string.Equals(originalProduct.GrangerNum, product.GrangerNum))
        {
            changes.Add(new Change
            {
                FieldChanged = "GrangerNum",
                Before = originalProduct.GrangerNum ?? String.Empty,
                After = product.GrangerNum ?? String.Empty
            });
        }
        if (!string.Equals(originalProduct.Description, product.Description))
        {
            changes.Add(new Change
            {
                FieldChanged = "Description",
                Before = originalProduct.Description,
                After = product.Description
            });
        }
        /*
        if (!string.Equals(originalProduct.Photo, product.Photo))
        {
            changes.Add(new Change
            {
                FieldChanged = "Photo",
                Before = originalProduct.Photo ?? String.Empty,
                After = product.Photo ?? String.Empty
            });
        }
        */
        if (originalProduct.ExpirationDate != product.ParsedDate)
        {
            changes.Add(new Change
            {
                FieldChanged = "ExpirationDate",
                Before = originalProduct.ExpirationDate?.ToString("MM/dd/yyyy") ?? String.Empty,
                After = product.ParsedDate?.ToString("MM/dd/yyyy") ?? String.Empty
            });
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

    public void AddInitialHistory(string userName, Product product)
    {
        List<Change> changes =
        [
            new Change
            {
                FieldChanged = "Name",
                Before = String.Empty,
                After = product.Name
            },
            new Change
            {
                FieldChanged = "Quantity",
                Before = String.Empty,
                After = product.Quantity.ToString()
            },
            new Change
            {
                FieldChanged = "Unit",
                Before = String.Empty,
                After = product.Unit.ToString()
            },
            new Change
            {
                FieldChanged = "GrangerNum",
                Before = String.Empty,
                After = product.GrangerNum ?? String.Empty
            },
            new Change
            {
                FieldChanged = "Description",
                Before = String.Empty,
                After = product.Description
            },
            /*
            new Change
            {
                FieldChanged = "Photo",
                Before = String.Empty,
                After = product.Photo ?? String.Empty
            },
            */
            new Change
            {
                FieldChanged = "ExpirationDate",
                Before = String.Empty,
                After = product.ExpirationDate?.ToString("MM/dd/yyyy") ?? String.Empty
            },
        ];

        var productHistory = new ProductHistory
        {
            CreatedDt = DateTime.Now,
            CreatedBy = userName,
            ProductId = product.ProductId,
            ChangeType = ProductHistory.changeType.update,
            ChangeJson = JsonSerializer.Serialize(changes),
            Comment = "created product"
        };

        _context.Add(productHistory);
        _context.SaveChanges();
    }

    /*
    public void AddRevertHistory(string userName, List<Change> changes, Product product)
    {
        List<Change> revertChanges = new List<Change>();

        foreach (Change change in changes)
        {
            string before;
            if (change.FieldChanged.Equals("Unit"))
            {
                before = product.Unit.ToString();
            }
            else if (change.FieldChanged.Equals("ExpirationDate"))
            {
                before = product.ExpirationDate.ToString("MM/dd/yyyy") ?? String.Empty;
            }
            else
            {
                PropertyInfo field = product?.GetType().GetProperty(change.FieldChanged);
                before = field.GetValue(product);
            }
        }
    }
    */
}