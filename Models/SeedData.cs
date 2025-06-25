using System.ComponentModel;
using System.Globalization;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.EntityFrameworkCore;
using EHSInventory.Services;
using EHSInventory.Utils;

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
                        DisplayOrder = 1,
                        Icon = "FirstAid.svg"
                    },
                    new ProductCategory
                    {
                        Name = "Wellness",
                        DisplayOrder = 2,
                        Icon = "Wellness.svg"
                    },
                      new ProductCategory
                      {
                          Name = "Ergonomic",
                          DisplayOrder = 3,
                          Icon = "Ergonomic.svg"
                      },
                    new ProductCategory
                    {
                        Name = "Extras",
                        DisplayOrder = 4,
                        Icon = "Extras.svg"
                    },
                    new ProductCategory
                    {
                        Name = "Hard Hats/EHS Incentives",
                        DisplayOrder = 5,
                        Icon = "HardHat.svg"
                    },
                    new ProductCategory
                    {
                        Name = "HazCom",
                        DisplayOrder = 6,
                        Icon = "HazCom.svg"
                    },
                    new ProductCategory
                    {
                        Name = "Safety Boots",
                        DisplayOrder = 7,
                        Icon = "SafetyBoots.svg"
                    },
                    new ProductCategory
                    {
                        Name = "Fall Protection",
                        DisplayOrder = 8,
                        Icon = "FallProtection.svg"
                    },
                    new ProductCategory
                    {
                        Name = "LOTO",
                        DisplayOrder = 9,
                        Icon = "LOTO.svg"
                    },
                    new ProductCategory
                    {
                        Name = "PPE",
                        DisplayOrder = 10,
                        Icon = "PPE.svg"
                    },
                    new ProductCategory
                    {
                        Name = "IH Cabinet",
                        DisplayOrder = 11,
                        Icon = "IHCabinet.svg"
                    }
                );

                context.SaveChanges();
            }

            if (!context.Products.Any())
            {
                var scope = app.ApplicationServices.CreateScope();
                var importerExporter = scope.ServiceProvider.GetRequiredService<ImporterExporter>();
                importerExporter.ImportProductsAsync("products.csv", "seeddata").GetAwaiter().GetResult();
            }
        }
    }
}