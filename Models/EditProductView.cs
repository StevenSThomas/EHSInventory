using System.ComponentModel.DataAnnotations;
using System.Globalization;
using Microsoft.IdentityModel.Tokens;

namespace EHSInventory.Models;

public class EditProductView : IValidatableObject
{
    public long? ProductId { get; set; }

    [Required(ErrorMessage = "A product name is required.")]
    [StringLength(255)]
    public string? Name { get; set; }
    
    [Range(0, int.MaxValue, ErrorMessage = "A positive quantity is required.")]
    public int Quantity { get; set; }

    public ProductUnit Unit { get; set; } = ProductUnit.Individual;

    [StringLength(50)]
    public string? GrangerNum { get; set; }

    [Required(ErrorMessage = "A product description is required.")]
    public string Description { get; set; } = String.Empty;

    public string? Photo { get; set; }
    public string? ExpirationDate { get; set; }
    public DateTime? ParsedDate { get; set; }

    [Required(ErrorMessage = "A comment explaining this change is required.")]
    public string? Comment { get; set; }

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        // validate ExpirationDate
        if (!ExpirationDate.IsNullOrEmpty())
        {
            string[] formats = { "M/d/yy", "MM/d/yy", "M/dd/yy", "MM/dd/yy", "M/d/yyyy", "MM/d/yyyy", "M/dd/yyyy", "MM/dd/yyyy" };
            DateTime dateTime;
            if (DateTime.TryParseExact(ExpirationDate.Trim(), formats, CultureInfo.InvariantCulture, DateTimeStyles.None, out dateTime))
            {
                ParsedDate = dateTime;
            }
            else
            {
                yield return new ValidationResult(
                    $"Invalid date. Date format: mm/dd/yyyy",
                    new[] { nameof(ExpirationDate) });
            }
        }

        // check for duplicate name, unit, and expiration date
            var context = (InventoryDbContext?)validationContext.GetService(typeof(InventoryDbContext));

        var duplicates = context?.Products.Where(prod =>
        prod.ProductId != ProductId
        && prod.Name != null
        && prod.Name.Equals(Name)
        && prod.Unit == Unit
        && prod.ExpirationDate.Equals(ParsedDate));

        if (duplicates != null && duplicates.Any())
        {
            yield return new ValidationResult(
                "A product with this name, unit, and expiration date already exists.",
                new[] { nameof(Name) });
        }
    }
}
