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

//Unit and ExpiryDate needed to be parsed for CatalogService
string UnitStr = row[4].Trim();
string ExpiryDateStr = row[5].Trim();

bool unitParsed = Enum.TryParse<ProductUnit>(UnitStr, true, out var unit);
DateTime? expiry = DateTime.TryParse(ExpiryDateStr, out var dt) ? dt : (DateTime?)null;

            try
            {
                string Category = row[0].Trim();
                string PartNumber = row[1].Trim();
                string Name = row[2].Trim();
                int Quantity = int.Parse(row[3].Trim(), CultureInfo.InvariantCulture);
                string Unit = row[4].Trim();
                string ExpiryDate = row[5].Trim();

                await _catalogService.AddProduct(
                    userName,
                    Category,
                    Name,
                    unit,
                    Quantity,
                    grangerNum: PartNumber,
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


//Exporter
    public async Task ExportProductsAsync(InventoryDbContext context, string fileName = "exported_products.csv")
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
