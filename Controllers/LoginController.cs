using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using EHSInventory.Models;

namespace EHSInventory.Controllers;

public class LoginController : Controller
{
    public IActionResult Index()
    {
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Index(string? returnUrl, [Bind("Username", "Password")] LoginView loginView)
    {
        if (!ModelState.IsValid)
        {
            return View(loginView);
        }

        string role = String.Empty;

        if (loginView.Username.Equals("safety") && loginView.Password.Equals("officer"))
        {
            role = "Safety Officer";
        }
        else if (loginView.Username.Equals("product") && loginView.Password.Equals("requester"))
        {
            role = "Product Requester";
        }

        if (role.Equals(String.Empty))
        {
            ModelState.AddModelError("Password", "Invalid username or password.");
            return View(loginView);
        }

        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.Name, loginView.Username),
            new Claim(ClaimTypes.Role, role)
        };

        var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);

        await HttpContext.SignInAsync(
            CookieAuthenticationDefaults.AuthenticationScheme,
            new ClaimsPrincipal(claimsIdentity)
        );

        return Redirect(returnUrl ?? "/");
    }

    [Route("/Logout")]
    public async Task<IActionResult> Logout()
    {
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        return Redirect("/");
    }
}