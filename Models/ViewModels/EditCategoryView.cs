using System.ComponentModel.DataAnnotations;

namespace EHSInventory.Models;

public class EditCategoryView
{
    public long? ProductCategoryId { get; set; }

    [Required]
    [StringLength(50)]
    public string? Name { get; set; }

    public int DisplayOrder { get; set; } = 0;

    [StringLength(255)]
    public string? Icon { get; set; } = String.Empty;

    [Required(ErrorMessage = "A comment is required explaining this change.")]
    public string Comment { get; set; } = String.Empty;

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        var context = (InventoryDbContext?)validationContext.GetService(typeof(InventoryDbContext));

        var duplicates = context?.ProductCategories.Where(cat => cat.ProductCategoryId != ProductCategoryId && cat.Name == Name);

        if (duplicates != null && duplicates.Any())
        {
            yield return new ValidationResult(
                "A category with this name already exists.",
                new[] { nameof(Name) });
        }
    }
}