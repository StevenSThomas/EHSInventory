using Microsoft.AspNetCore.Mvc;
using EHSInventory.Models;
using EHSInventory.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace EHSInventory.Controllers;

// [Authorize(Roles = "Product Requester")]
public class OrdersController : Controller
{
    private InventoryDbContext _context;
    private ShoppingService _shoppingService;

    public OrdersController(InventoryDbContext context, ShoppingService shoppingService)
    {
        _context = context;
        _shoppingService = shoppingService;
    }

    [Route("/Cart")]
    [Authorize(Roles = "Product Requester")]
    public async Task<IActionResult> Cart()
    {
        return View(new CartView
        {
            Items = await _shoppingService.ListShoppingCartItems(User.Identity.Name)
        });
    }

    [ActionName("AddToCart")]
    [Route("/AddToCart")]
    public async Task<IActionResult> AddToCart(AddToCartView view)
    {
        return View(view);
    }

    [HttpPost, ActionName("AddToCartPost")]
    public async Task<IActionResult> AddToCartPost(AddToCartView view)
    {
        bool success = await _shoppingService.AddToCart(User.Identity?.Name, view.ProductName, view.Unit, view.Count);
        if (success)
        {
            return NoContent();
        }
        return BadRequest("something went wrong");
    }

    /*
    [HttpPost]
    [Route("/PlaceOrder")]
    public async Task<IActionResult> PlaceOrder(string userName)
    {
        bool success = await _shoppingService.PlaceOrder(userName);
        if (success)
        {
            return Ok("it worked");
        }
        return BadRequest("something went wrong");
    }
    */
}