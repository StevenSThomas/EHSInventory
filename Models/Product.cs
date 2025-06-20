using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;

namespace EHSInventory.Models
{
    public enum ProductUnit
    {
        Individual = 0,
        Box = 1,
        Case = 2,
        Pack = 3,
        Roll=4
    }

    public class Product : IValidatableObject
    {
        public long? ProductId { get; set; }

        public ProductCategory? Category { get; set; }

        [Required(ErrorMessage = "A product name is required.")]
        [Column(TypeName = "VARCHAR")]
        [StringLength(255)]
        public string? Name { get; set; }

        public ProductUnit Unit { get; set; } = ProductUnit.Individual;

        [Range(0, int.MaxValue, ErrorMessage = "A positive quantity is required.")]
        public int Quantity { get; set; } = 0;

        public int DisplayOrder { get; set; } = 0;

        [Column(TypeName = "VARCHAR")]
        [StringLength(50)]
        public string? GrangerNum { get; set; }

        [Column(TypeName = "TEXT")]
        [Required(ErrorMessage = "A product description is required.")]
        public string Description { get; set; } = String.Empty;

        public string? Photo { get; set; }

        [Column(TypeName = "DATE")]
        public DateTime? ExpirationDate { get; set; }

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
                    new[] { nameof(Name)});
            }
        }
    }
}