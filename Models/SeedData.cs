using Microsoft.EntityFrameworkCore;

namespace EHSInventory.Models
{
    public static class SeedData
    {
        public static void EnsurePopulated(IApplicationBuilder app)
        {
            InventoryDbContext context = app.ApplicationServices.CreateScope().ServiceProvider.GetRequiredService<InventoryDbContext>();
            if (context.Database.GetPendingMigrations().Any())
            {
                context.Database.Migrate();
            }

            if (!context.ProductCategories.Any())
            {
                context.ProductCategories.AddRange(
                    new ProductCategory
                    {
                        Name = "First Aid",
                        DisplayOrder = 1
                    },
                    new ProductCategory
                    {
                        Name = "Wellness",
                        DisplayOrder = 2
                    },
                      new ProductCategory
                    {
                        Name = "Ergonomic",
                        DisplayOrder = 3
                    }
                );
            }

            context.SaveChanges();
        }
    }
}