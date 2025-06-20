using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EHSInventory.Models
{
    public class ProductCategory : IValidatableObject
    {

        public long? ProductCategoryId { get; set; }

        [Required]
        [Column(TypeName = "VARCHAR")]
        [StringLength(50)]
        public string? Name { get; set; }

        public int DisplayOrder { get; set; } = 0;

        [Column(TypeName = "VARCHAR")]
        [StringLength(255)]
        public string? Icon { get; set; } = String.Empty;

        public ICollection<Product> Products { get; } = new List<Product>();

        public void AddProduct(Product product)
        {
            var NextDisplayOrder = 0;
            // TODO: find the maximum display order for all products in this category
            //   NextDisplayOrder should be maximum display order + 1

            
            product.DisplayOrder = NextDisplayOrder;
            Products.Add(product);
        }

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
}