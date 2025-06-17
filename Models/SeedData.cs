using Microsoft.AspNetCore.Authorization.Infrastructure;
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
                    },
                    new ProductCategory
                    {
                        Name = "Extras",
                        DisplayOrder = 4
                    },
                    new ProductCategory
                    {
                        Name = "Hard Hats/EHS Incentives",
                        DisplayOrder = 5
                    },
                    new ProductCategory
                    {
                        Name = "HazCom",
                        DisplayOrder = 6
                    },
                    new ProductCategory
                    {
                        Name = "Safety Boots",
                        DisplayOrder = 7
                    },
                    new ProductCategory
                    {
                        Name = "Fall Protection",
                        DisplayOrder = 8
                    },
                    new ProductCategory
                    {
                        Name = "LOTO",
                        DisplayOrder = 9
                    },
                    new ProductCategory
                    {
                        Name = "PPE",
                        DisplayOrder = 10
                    },
                    new ProductCategory
                    {
                        Name = "IH Cabinet",
                        DisplayOrder = 11
                    }
                );
            }

            context.SaveChanges();
        }
    }
}