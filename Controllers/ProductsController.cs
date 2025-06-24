using Microsoft.AspNetCore.Mvc;
using EHSInventory.Models;
using EHSInventory.Services;
using Microsoft.EntityFrameworkCore;

namespace EHSInventory.Controllers;

public class ProductsController : Controller
{
    private readonly InventoryDbContext _context;
    private readonly ICatalogService _catalogService;

    public ProductsController(InventoryDbContext context, ICatalogService catalogService)
    {
        _context = context;
        _catalogService = catalogService;
    }

    public async Task<IActionResult> Index(long id) // edit
    {
        var product = await _context.Products.FindAsync(id);
        if (product == null)
        {
            return NotFound();
        }

        return View(product);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Index(long id, [Bind("ProductId", "Category", "Name", "Unit", "Quantity", "DisplayOrder", "GrangerNum", "Description", "Photo", "ExpirationDate")] Product product, string comment)
    {
        if (product.ProductId != id)
        {
            return NotFound();
        }

        if (ModelState.IsValid)
        {
            bool success = await _catalogService.UpdateProduct("placeholder", product, comment);
            if (product.Category != null)
            {
                return Redirect($"/Categories/{product.Category.ProductCategoryId}"); // this doesn't work
            }
            else return Redirect("/Categories");
        }
        return View(product);
    }

    [HttpPost]
    public async Task<IActionResult> SetQuantity(long id, int newQuantity, string comment)
    {
        Console.WriteLine(newQuantity);
        Console.WriteLine(comment);
        bool success = await _catalogService.SetProductQuantity("placeholder", id, newQuantity, comment);
        if (success)
        {
            var product = await _context.Products.Include(p => p.Category).FirstAsync(p => p.ProductId == id);
            return Redirect($"/Categories/{product?.Category?.ProductCategoryId}");
        }
        return NotFound();
    }

    public async Task<IActionResult> Delete(long id)
    {
        var product = await _context.Products.FindAsync(id);
        if (product == null)
        {
            return NotFound();
        }
        return View(product);
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(long id)
    {
        var product = await _context.Products.Include(p => p.Category).FirstOrDefaultAsync(p => p.ProductId == id);
        var categoryId = product?.Category?.ProductCategoryId;
        if (product != null)
        {
            await _catalogService.FixOrderAsync(id);
            _context.Remove(product);
            await _context.SaveChangesAsync();

            if (categoryId != null)
            {
                return Redirect($"/Categories/{categoryId}");
            }
            return Redirect($"/Categories");
        }
        return NotFound();

    }

    [HttpPost, ActionName("Reorder")]
    public async Task<IActionResult> Reorder(long id, int newPosition)
    {
        var success = await _catalogService.ReorderProductAsync(id, newPosition);

        if (!success)
        {
            return NotFound("Invalid product ID or position.");
        }

        return Ok("Success");
    }
}