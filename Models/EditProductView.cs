using System.ComponentModel.DataAnnotations;

namespace EHSInventory.Models;

public class EditProductView : IValidatableObject
{
    public long? ProductId { get; set; }

    [Required(ErrorMessage = "A product name is required.")]
    [StringLength(255)]
    public string? Name { get; set; }

    public ProductUnit Unit { get; set; } = ProductUnit.Individual;

    [StringLength(50)]
    public string? GrangerNum { get; set; }

    [Required(ErrorMessage = "A product description is required.")]
    public string Description { get; set; } = String.Empty;

    public string? Photo { get; set; }
    public DateTime? ExpirationDate { get; set; }

    [Required(ErrorMessage = "A comment explaining this change is required.")]
    public string? Comment { get; set; }

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        var context = (InventoryDbContext?)validationContext.GetService(typeof(InventoryDbContext));

        var duplicates = context?.Products.Where(prod =>
        prod.ProductId != ProductId
        && prod.Name != null
        && prod.Name.Equals(Name)
        && prod.Unit == Unit);

        if (duplicates != null && duplicates.Any())
        {
            yield return new ValidationResult(
                "A product with this name and unit already exists.",
                new[] { nameof(Name) });
        }
    }
}