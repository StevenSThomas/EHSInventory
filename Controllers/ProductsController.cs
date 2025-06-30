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
        var editProductView = new EditProductView
        {
            ProductId = id,
            Name = product.Name,
            Quantity = product.Quantity,
            Unit = product.Unit,
            GrangerNum = product.GrangerNum,
            Description = product.Description,
            Photo = product.Photo,
            ExpirationDate = product.ExpirationDate?.ToString("MM/dd/yyyy") ?? String.Empty
        };

        return View(editProductView);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Index(long id, EditProductView product, string comment)
    {
        if (product.ProductId != id)
        {
            return NotFound();
        }

        if (ModelState.IsValid)
        {
            await _catalogService.UpdateProduct("placeholder", product, comment);
            return Redirect("/Categories");
        }
        return View(product);
    }

    [HttpPost]
    public async Task<IActionResult> SetQuantity(long id, int newQuantity, string comment)
    {
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

        return View(new DeleteConfirmationView {Name = product.Name, Comment = null });
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(long id, DeleteConfirmationView deleteConfirmationView)
    {
        var product = await _context.Products.Include(p => p.Category).FirstOrDefaultAsync(p => p.ProductId == id);
        var categoryId = product?.Category?.ProductCategoryId;
        if (product == null)
        {
            return NotFound();
        }

        if (deleteConfirmationView.Comment == null)
        {
            return View(new DeleteConfirmationView { Name = product.Name, Comment = null });
        }

        await _catalogService.DeleteProduct("placeholder", id, deleteConfirmationView.Comment);
        
        return Redirect($"/Categories/{categoryId}");

    }

    [HttpPost]
    public async Task<IActionResult> SetDisplayOrder(long id, int newPosition)
    {
        var success = await _catalogService.SetProductDisplayOrder("placeholder", id, newPosition);

        if (!success)
        {
            return NotFound("Invalid product ID or position.");
        }

        return Ok("Success");
    }
}