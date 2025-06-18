using Microsoft.AspNetCore.Mvc;
using EHSInventory.Models;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Humanizer;

namespace EHSInventory.Controllers;

public class ProductsController : Controller
{
    private readonly InventoryDbContext _context;

    public ProductsController(InventoryDbContext context)
    {
        _context = context;
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
    public async Task<IActionResult> Index(long id, [Bind("ProductId", "Category", "Name", "Unit", "Quantity", "DisplayOrder", "GrangerNum", "Description", "Photo", "ExpirationDate")] Product product)
    {
        if (product.ProductId != id)
        {
            return NotFound();
        }

        if (ModelState.IsValid)
        {
            _context.Update(product);
            await _context.SaveChangesAsync();
            if (product.Category != null)
            {
                return Redirect($"/Categories/{product.Category.ProductCategoryId}"); // this doesn't work
            }
            else return Redirect("/Categories");
        }
        return View(product);
    }
}