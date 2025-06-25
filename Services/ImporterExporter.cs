using System.Globalization;
using EHSInventory.Models;
using EHSInventory.Services;
using Microsoft.EntityFrameworkCore;

namespace EHSInventory.Utils;

public class ImporterExporter
{
    private readonly ICatalogService _catalogService;
    private readonly IWebHostEnvironment _env;
    private readonly ILogger<ImporterExporter> _logger;

    public ImporterExporter(ICatalogService catalogService, IWebHostEnvironment env, ILogger<ImporterExporter> logger)
    {
        _catalogService = catalogService;
        _env = env;
        _logger = logger;
    }

    public async Task ImportProductsAsync(string fileName = "products.csv", string userName = "importer")
    {
        string path = Path.Combine(_env.ContentRootPath, "Data", fileName);

        if (!File.Exists(path))
        {
            _logger.LogError("No CSV file found at path: {Path}", path);
            return;
        }

        using var reader = new StreamReader(path);
        bool isHeader = true;

        while (!reader.EndOfStream)
        {
            var line = await reader.ReadLineAsync();
            if (string.IsNullOrWhiteSpace(line)) continue;

            if (isHeader)
            {
                isHeader = false;
                continue;
            }

            var row = line.Split(',');

            if (row.Length < 6)
            {
                _logger.LogWarning("Missing data from row: {Row}", line);
                continue;
            }

            try
            {
                string category = row[0].Trim();
                string partNumber = row[1].Trim();
                string name = row[2].Trim();

                // Quantity
                if (!int.TryParse(row[3].Trim(), NumberStyles.Integer, CultureInfo.InvariantCulture, out int quantity))
                {
                    _logger.LogWarning("Invalid quantity '{Quantity}' for product '{Name}'", row[3], name);
                    quantity = 0; // or skip the row if you prefer
                }

                // Unit (optional)
                string unitStr = row[4].Trim();
                ProductUnit unit = ProductUnit.Individual; // default
                if (!string.IsNullOrWhiteSpace(unitStr))
                {
                    if (!Enum.TryParse<ProductUnit>(unitStr, true, out unit))
                    {
                        _logger.LogWarning("Unknown unit '{Unit}' for product '{Name}', using default.", unitStr, name);
                    }
                }

                // Expiration Date (optional)
                string expiryStr = row[5].Trim();
                DateTime? expiry = null;
                if (!string.IsNullOrWhiteSpace(expiryStr))
                {
                    if (!DateTime.TryParse(expiryStr, out DateTime parsedDate))
                    {
                        _logger.LogWarning("Invalid expiration date '{Date}' for product '{Name}'", expiryStr, name);
                    }
                    else
                    {
                        expiry = parsedDate;
                    }
                }

                await _catalogService.AddProduct(
                    userName,
                    category,
                    name,
                    unit,
                    quantity,
                    grangerNum: partNumber,
                    expirationDate: expiry,
                    description: null,
                    photo: null
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error importing row: {Row}", line);
            }

        }

        _logger.LogInformation("Finished importing products.");
    }

    public async Task ExportProductsAsync(InventoryDbContext context, string fileName = "export.csv")
    {
        string path = Path.Combine(_env.ContentRootPath, "Data", fileName);

        using var writer = new StreamWriter(path);
        await writer.WriteLineAsync("Category,PartNumber,Name,Quantity,Unit,ExpirationDate");

        var products = context.Products.Include(p => p.Category).ToList();

        foreach (var p in products)
        {
            string line = string.Join(",",
                p.Category?.Name ?? "",
                p.GrangerNum ?? "",
                p.Name ?? "",
                p.Quantity.ToString(),
                p.Unit.ToString(),
                p.ExpirationDate?.ToString("yyyy-MM-dd") ?? ""
            );

            await writer.WriteLineAsync(line);
        }

        _logger.LogInformation("Exported {Count} products to {Path}", products.Count, path);
    }
}
