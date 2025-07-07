using Microsoft.AspNetCore.Mvc;
using EHSInventory.Models;
using EHSInventory.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;

namespace EHSInventory.Controllers;

[Authorize(Roles = "Safety Officer")]
public class ProductsController : Controller
{
    private readonly InventoryDbContext _context;
    private readonly ICatalogService _catalogService;

    public ProductsController(InventoryDbContext context, ICatalogService catalogService)
    {
        _context = context;
        _catalogService = catalogService;
    }

    public async Task<IActionResult> Index(long id)
    {
        var product = await _context.Products.Include(p => p.Category).FirstAsync(p => p.ProductId == id);
        if (product == null)
        {
            return NotFound();
        }
        var productView = new ProductView
        {
            ProductId = id,
            Name = product.Name,
            Quantity = product.Quantity,
            Unit = product.Unit,
            GrangerNum = product.GrangerNum,
            Description = product.Description,
            Photo = product.Photo,
            ExpirationDate = product.ExpirationDate,
            CategoryId = product.Category.ProductCategoryId,
            CategoryName = product.Category.Name
        };

        productView.ProductHistories = await _catalogService.GetProductHistories(id);

        return View(productView);
    }

    public async Task<IActionResult> Edit(long id) // edit
    {
        var product = await _context.Products.Include(p => p.Category).FirstAsync(p => p.ProductId == id);
        if (product == null)
        {
            return NotFound();
        }
        var editProductView = new EditProductView
        {
            ProductId = id,
            Name = product.Name,
            // Quantity = product.Quantity,
            Unit = product.Unit,
            GrangerNum = product.GrangerNum,
            Description = product.Description,
            Photo = product.Photo,
            ExpirationDate = product.ExpirationDate?.ToString("MM/dd/yyyy") ?? String.Empty,
            CategoryId = product.Category.ProductCategoryId
        };

        return View(editProductView);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(long id, EditProductView product, string comment)
    {
        if (product.ProductId != id)
        {
            return NotFound();
        }

        if (ModelState.IsValid)
        {
            await _catalogService.UpdateProduct(User.Identity.Name, product, comment);
            return Redirect($"/Products/{product.ProductId}");
        }

        return View(product);
    }

    public async Task<IActionResult> SetQuantity(long id)
    {
        var product = await _context.Products.FindAsync(id);

        if (product == null) return NotFound();

        SetQuantityView view = new SetQuantityView
        {
            ProductId = id,
            ProductName = product.Name,
            NewQuantity = product.Quantity,
            Unit = product.Unit
        };

        return View(view);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SetQuantity(long id, SetQuantityView view)
    {
        if (!ModelState.IsValid)
        {
            return View(view);
        }

        bool success = await _catalogService.SetProductQuantity(User.Identity.Name, id, view.NewQuantity, view.Unit, view.Comment);
        if (success)
        {
            return Redirect($"/Products/{view.ProductId}");
        }
        return NotFound();
    }

    public async Task<IActionResult> Delete(long id)
    {
        var product = await _context.Products.Include(p => p.Category).FirstAsync(p => p.ProductId == id);
        if (product == null)
        {
            return NotFound();
        }

        return View(new DeleteConfirmationView
        {
            Name = product.Name,
            Comment = null,
            CategoryId = product.Category.ProductCategoryId,
            ProductId = id
        });
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(long id, DeleteConfirmationView deleteConfirmationView)
    {
        var product = await _context.Products.FindAsync(id);
        if (product == null)
        {
            return NotFound();
        }

        if (deleteConfirmationView.Comment == null)
        {
            return View(deleteConfirmationView);
        }

        await _catalogService.DeleteProduct(User.Identity.Name, id, deleteConfirmationView.Comment);

        return Redirect($"/Categories/{deleteConfirmationView.CategoryId}");

    }

    [HttpPost]
    public async Task<IActionResult> SetDisplayOrder(long id, int newPosition)
    {
        var success = await _catalogService.SetProductDisplayOrder(User.Identity.Name, id, newPosition);

        if (!success)
        {
            return NotFound("Invalid product ID or position.");
        }

        return Ok("Success");
    }
}