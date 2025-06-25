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
                _logger.LogWarning("Row does not have enough columns: {Line}", line);
                continue;
            }

            try
            {
                string category = row[0].Trim();
                string partNumber = row[1].Trim();
                string name = row[2].Trim();
                string quantityStr = row[3].Trim();
                string unitStr = row[4].Trim().Trim('"');
                string expiryStr = row[5].Trim();

                if (!int.TryParse(quantityStr, out int quantity))
                {
                    _logger.LogWarning("Invalid quantity in row: {Line}", line);
                    quantity = 0;
                }

                ProductUnit unit = ProductUnit.Individual;
                if (!string.IsNullOrWhiteSpace(unitStr) && Enum.TryParse<ProductUnit>(unitStr, true, out var parsedUnit))
                {
                    unit = parsedUnit;
                }
                else if (!string.IsNullOrWhiteSpace(unitStr))
                {
                    _logger.LogWarning("Invalid unit '{Unit}' in row: {Line}", unitStr, line);
                }


                DateTime? expiry = null;
                if (!string.IsNullOrWhiteSpace(expiryStr) && expiryStr != "-")
                {
                    if (!DateTime.TryParse(expiryStr, out var dt))
                    {
                        _logger.LogWarning("Invalid expiration date in row: {Line}", line);
                    }
                    else
                    {
                        expiry = dt;
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

                _logger.LogInformation("Imported product: {Name} ({Category})", name, category);
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
