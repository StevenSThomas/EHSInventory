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
        public string? IconUrl { get; set; } = String.Empty;
    }
}