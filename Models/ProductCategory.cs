using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EHSInventory.Models
{
    public class ProductCategory
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

        // TODO: Products should return and collection if there
        // are not products assigned to the category
        public ICollection<Product>? Products { get; }
        
    }
}