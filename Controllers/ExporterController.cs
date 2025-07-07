using Microsoft.AspNetCore.Mvc;
using EHSInventory.Utils;
using Microsoft.EntityFrameworkCore;
using EHSInventory.Models;

public class CatalogController : Controller
{
    private readonly ImporterExporter _importerExporter;
    private readonly InventoryDbContext _context;

    public CatalogController(ImporterExporter importerExporter, InventoryDbContext context)
    {
        _importerExporter = importerExporter;
        _context = context;
    }

    [HttpGet]
    public IActionResult Exporter()
    {
        string fileName = "export.csv";
        _importerExporter.ExportProducts(_context, fileName);
        string path = Path.Combine(Directory.GetCurrentDirectory(), "Data", fileName);
        if (!System.IO.File.Exists(path))
        {
            return NotFound("Exported file not found.");
        }

        var fileBytes = System.IO.File.ReadAllBytes(path);
        return File(fileBytes, "text/csv", fileName);
    }
}
