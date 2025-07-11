using EHSInventory.Models;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.EntityFrameworkCore;
using EHSInventory.Persistence;
using EHSInventory.Services;
using Microsoft.AspNetCore.Authentication.Cookies;
using EHSInventory.Utils;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
//builder.Services.AddDbContext
builder.Services.AddDbContext<InventoryDbContext>(
    options => options.UseSqlite(
        builder.Configuration.GetConnectionString("DefaultConnection")
    )
);
builder.Services.AddControllersWithViews();

builder.Services.AddScoped<ImporterExporter>();
builder.Services.AddScoped<ICatalogService, CatalogService>();
builder.Services.AddScoped<IProductRepository, ProductRepository>();
builder.Services.AddScoped<ICategoryRepository, CategoryRepository>();
builder.Services.AddScoped<ShoppingService>();
builder.Services.AddScoped<OrderService>();

builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Login";
        options.SlidingExpiration = true;
    });

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
}
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapControllerRoute(
    name: "orders",
    pattern: "Orders/{action}",
    defaults: new { controller = "Orders", action = "Index" }
);

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Categories}/{id?}/{action=Index}");

SeedData.EnsurePopulated(app);

app.Run();
