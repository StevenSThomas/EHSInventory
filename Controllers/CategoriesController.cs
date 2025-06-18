using Microsoft.AspNetCore.Mvc;
using EHSInventory.Models;
using Microsoft.EntityFrameworkCore;

namespace EHSInventory.Controllers;

public class CategoriesController : Controller
{
    private readonly InventoryDbContext _context;

    public CategoriesController(InventoryDbContext context)
    {
        _context = context;
    }

    public async Task<IActionResult> Index(long? id)
    {
        if (id == null)
        {
            var allCategories = await _context.ProductCategories.ToListAsync();

            var orderedCategoryList = allCategories.OrderBy(cat => cat.DisplayOrder).ToList();

            return View("List", orderedCategoryList);
        }

        var category = await _context.ProductCategories.FindAsync(id);

        if (category != null)
        {
            ViewData["Name"] = category.Name;
            ViewData["Icon"] = category.Icon;
            ViewData["CategoryId"] = category.ProductCategoryId;

            var products = _context.Products.Where<Product>(p => p.Category != null && p.Category.ProductCategoryId == id);

            return View(products);
        }
        else
        {
            return NotFound();
        }
    }

    public IActionResult Create()
    {
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create([Bind("ProductCategoryId", "Name", "DisplayOrder", "Icon")] ProductCategory category)
    {
        if (ModelState.IsValid)
        {
            if (category.Icon == null) category.Icon = string.Empty;
            _context.Add(category);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
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
        ViewData["id"] = id;

        return View(category);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(long id, [Bind("ProductCategoryId", "Name", "DisplayOrder", "Icon")] ProductCategory category)
    {
        if (category.ProductCategoryId != id)
        {
            return NotFound();
        }

        if (ModelState.IsValid)
        {
            if (category.Icon == null) category.Icon = string.Empty;
            _context.Update(category);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
        return View(category);
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
        return View(category);
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(long id)
    {
        var category = await _context.ProductCategories.FindAsync(id);
        if (category != null)
        {
            _context.Remove(category);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        return View(category);
    }

    public async Task<IActionResult> Add(long id)
    {
        var category = await _context.ProductCategories.FindAsync(id);

        if (category == null)
        {
            return NotFound();
        }
        ViewData["Category Name"] = category.Name;

        return View();
    }

    [HttpPost, ActionName("Add")]
    [ValidateAntiForgeryToken]

    public async Task<IActionResult> AddConfirmed(long id, [Bind("ProductId", "Category", "Name", "Unit", "Quantity", "DisplayOrder", "GrangerNum", "Description", "Photo", "ExpirationDate")] Product product)
    {
        var category = await _context.ProductCategories.FindAsync(id);

        if (ModelState.IsValid)
        {
            if (category == null)
            {
                return NotFound();
            }
            category.Products.Add(product);

            _context.Products.Add(product);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index), new { id = id });
        }

        return View(product);
    }
}