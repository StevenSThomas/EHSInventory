using Microsoft.AspNetCore.Mvc;
using EHSInventory.Models;
using Microsoft.EntityFrameworkCore;

namespace EHSInventory.Controllers;

public class CategoryController : Controller
{
    private readonly InventoryDbContext _context;

    public CategoryController(InventoryDbContext context)
    {
        _context = context;
    }

    public async Task<IActionResult> List()
    {
        var allCategories = await _context.ProductCategories.ToListAsync();

        var orderedCategoryList = allCategories.OrderBy(cat => cat.DisplayOrder).ToList();

        return View(orderedCategoryList);
        }

    public async Task<IActionResult> Index(long id)
    {
        var category = await _context.ProductCategories.FindAsync(id);

        if (category != null)
        {
            ViewData["Name"] = category.Name;
            ViewData["Icon"] = category.Icon;
        }
        else
        {
            return NotFound();
        }
        // probably list items here
        return View();
    }

    public IActionResult Create() {
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
            return RedirectToAction(nameof(List));
        }
        return View(category);
    }

    public async Task<IActionResult> Edit(long? id) {
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
            return RedirectToAction(nameof(List));
        }
        return View(category);
    }

    public async Task<IActionResult> Delete(long? id) {
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
            return RedirectToAction(nameof(List));
        }

        return View(category);
    }
}