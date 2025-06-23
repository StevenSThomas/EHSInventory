using System.ComponentModel;
using System.Globalization;
using Microsoft.AspNetCore.Authorization.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Update.Internal;

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
                // load the Data/products.csv
                string path = Path.Combine(app.ApplicationServices.GetRequiredService<IWebHostEnvironment>().ContentRootPath, "Data", "products.csv");
                using var reader = new StreamReader(path);
                bool isHeader = true;
                // iterate over each row in the products csv
                while (!reader.EndOfStream)
                {
                    var line = reader.ReadLine();
                    if (line == null)
                    {
                        continue;
                    }

                    if (isHeader)
                    {
                        isHeader = false;
                        continue;
                    }

                    var row = line.Split(',');
                    string Category = row[0].Trim();
                    string PartNumber = row[1].Trim();
                    string Name = row[2].Trim();
                    int Quantity = int.Parse(row[3].Trim(), CultureInfo.InvariantCulture);
                    string Unit = row[4].Trim();
                    string ExpiryDate = row[5].Trim();

                    //  -- loop -- 
                    ProductCategory category = context.ProductCategories.Where(category => category.Name == Category).First();
                    category.AddProduct(new Product
                    {
                        Category = category,
                        Name = Name,
                        Quantity = Quantity,
                        PartNumber = PartNumber,
                        Unit = Unit
                        ExpiryDate = ExpiryDate
                    });
                    context.SaveChanges();
                }
            }
        }
    }
}