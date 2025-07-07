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
    private OrderService _orderService;

    public OrdersController(InventoryDbContext context, ShoppingService shoppingService, OrderService orderService)
    {
        _context = context;
        _shoppingService = shoppingService;
        _orderService = orderService;
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
    // [Route("/AddToCart")]
    public async Task<IActionResult> AddToCart(AddToCartView view)
    {
        view.Count = await _shoppingService.GetSpecificItemCount(User.Identity.Name, view.ProductName, view.Unit);

        return View(view);
    }

    [HttpPost, ActionName("AddToCartPost")]
    public async Task<IActionResult> AddToCartPost(AddToCartView view)
    {
        bool success = await _shoppingService.AddToCart(User.Identity?.Name, view.ProductName, view.Unit, view.Count);
        if (success)
        {
            return Redirect($"/Categories/{view.CategoryId}");
        }
        return NotFound("Invalid product name/unit.");
    }

    [HttpPost]
    public async Task<IActionResult> DeleteItem(string productName, ProductUnit unit)
    {
        bool success = await _shoppingService.RemoveFromCart(User.Identity?.Name, productName, unit);
        if (success)
        {
            return RedirectToAction(nameof(Cart));
        }
        return NotFound("Invalid product name/unit.");
    }

    [HttpPost]
    public async Task<IActionResult> ClearCart()
    {
        bool success = await _shoppingService.ClearCart(User.Identity?.Name);
        if (success)
        {
            return RedirectToAction(nameof(Cart));
        }
        return NotFound("One of your cart items was invalid!");
    }

    [HttpPost]
    public async Task<IActionResult> PlaceOrder()
    {
        Order order = await _shoppingService.PlaceOrder(User.Identity?.Name);
        if (order != null)
        {
            return RedirectToAction(nameof(OrderConfirmed), order);
        }
        return NotFound("something went wrong, user's cart may not exist");
    }

    public async Task<IActionResult> OrderConfirmed(Order order)
    {
        ViewData["items"] = await _orderService.ListOrderItems(order.OrderId);

        return View(order);
    }
}