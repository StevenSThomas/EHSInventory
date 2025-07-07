using Microsoft.AspNetCore.Mvc;
using EHSInventory.Models;
using Microsoft.EntityFrameworkCore;
using EHSInventory.Services;
using Microsoft.AspNetCore.Authorization;

namespace EHSInventory.Controllers;

[Authorize(Roles = "Safety Officer, Product Requester")]
public class CategoriesController : Controller
{
    private readonly InventoryDbContext _context;
    private readonly ICatalogService _catalogService;
    private readonly ShoppingService _shoppingService;

    public CategoriesController(InventoryDbContext context, ICatalogService catalogService, ShoppingService shoppingService)
    {
        _context = context;
        _catalogService = catalogService;
        _shoppingService = shoppingService;
    }

    public async Task<IActionResult> Index(long? id)
    {
        if (id == null || id < 1)
        {
            id = 1;
        }

        List<Product> products;
        if (User.IsInRole("Safety Officer")) products = await _catalogService.ListProducts(id);
        else products = await _shoppingService.ListProducts(id);

        if (products != null)
        {

            var categoryInfo = new CategoryView
            {
                AllCategories = await _catalogService.ListCategories(),
                CurrentCategory = await _context.ProductCategories.FindAsync(id),
                Products = products
            };

            return View(categoryInfo);
        }

        return NotFound();
    }

    [Route("CreateCategory")]
    public IActionResult Create()
    {
        return View();
    }

    [Route("CreateCategory")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create([Bind("ProductCategoryId", "Name", "DisplayOrder", "Icon")] ProductCategory category)
    {
        if (ModelState.IsValid)
        {
            await _catalogService.AddCategory(User.Identity.Name, category);
            return Redirect($"/Categories/{category.ProductCategoryId}");
        }

        return View(category);
    }

    public async Task<IActionResult> Edit(long? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var category = await _context.ProductCategories.FindAsync(id);
        if (category == null)
        {
            return NotFound();
        }
        var editCategoryView = new EditCategoryView
        {
            ProductCategoryId = category.ProductCategoryId,
            Name = category.Name,
            DisplayOrder = category.DisplayOrder,
            Icon = category.Icon
        };

        return View(editCategoryView);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(long id, EditCategoryView editCategoryView)
    {
        if (editCategoryView.ProductCategoryId != id)
        {
            return NotFound();
        }

        if (ModelState.IsValid)
        {
            await _catalogService.UpdateCategory(User.Identity.Name, editCategoryView, editCategoryView.Comment);
            return RedirectToAction(nameof(Index));
        }
        return View(editCategoryView);
    }

    public async Task<IActionResult> Delete(long? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var category = await _context.ProductCategories.FindAsync(id);
        if (category == null)
        {
            return NotFound();
        }

        var deleteConfirmationView = new DeleteConfirmationView
        {
            Name = category.Name,
            Comment = null
        };

        return View(deleteConfirmationView);
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(long id, DeleteConfirmationView deleteConfirmationView)
    {
        var category = await _context.ProductCategories.FindAsync(id);
        if (deleteConfirmationView.Comment == null)
        {
            return View(new DeleteConfirmationView { Name = category?.Name, Comment = null });
        }

        bool success = await _catalogService.DeleteCategory(User.Identity.Name, id, deleteConfirmationView.Comment);
        if (success)
        {
            return RedirectToAction(nameof(Index), new { id = 1 });
        }

        return NotFound();
    }

    public async Task<IActionResult> Add(long id)
    {
        var category = await _context.ProductCategories.FindAsync(id);

        if (category == null)
        {
            return NotFound();
        }
        ViewData["Category Name"] = category.Name;

        return View(new EditProductView
        {
            CategoryId = id
        });
    }

    [HttpPost, ActionName("Add")]
    [ValidateAntiForgeryToken]

    public async Task<IActionResult> AddConfirmed(long id, EditProductView productView)
    {
        if (ModelState.IsValid)
        {
            bool success = await _catalogService.AddProduct(User.Identity.Name, id, productView);
            if (success)
            {
                return Redirect($"/Categories/{id}");
            }
        }

        var category = await _context.ProductCategories.FindAsync(id);

        if (category == null)
        {
            return NotFound();
        }
        ViewData["Category Name"] = category.Name;

        return View(productView);
    }
}