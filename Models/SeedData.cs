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

            context.ProductCategories.AddRange(
                new ProductCategory
                {
                    Name = "test",
                    DisplayOrder = 1
                },
                new ProductCategory
                {
                    Name = "test 2",
                    DisplayOrder = 2
                },
                new ProductCategory
                {
                    Name = "cool stuff",
                    DisplayOrder = 3
                },
                new ProductCategory
                {
                    Name = "Referees",
                    DisplayOrder = 4
                }
            );

            context.SaveChanges();
        }
    }
}