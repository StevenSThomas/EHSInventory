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
        Roll = 4,
        Bulk = 5,
        Pair = 6
    }

    public class Product
    {
        public long? ProductId { get; set; }

        public ProductCategory? Category { get; set; }

        [Required(ErrorMessage = "A product name is required.")]
        [Column(TypeName = "VARCHAR")]
        [StringLength(255)]
        public string? Name { get; set; }

        [EnumDataType(typeof(ProductUnit))]
        public ProductUnit Unit { get; set; } = ProductUnit.Individual;

        [Range(0, int.MaxValue, ErrorMessage = "A positive quantity is required.")]
        public int Quantity { get; set; } = 0;

        public int DisplayOrder { get; set; } = 0;

        [Column(TypeName = "VARCHAR")]
        [StringLength(50)]
        public string? GrangerNum { get; set; }

        [Column(TypeName = "TEXT")]
        [Required(ErrorMessage = "A product description is required.")]
        // Note: I think we said description shouldn't be required, but I would have to re-migrate the database to make it not required.
        // For now, I'm just passing in String.Empty instead of null for the description to meet this requirement.
        public string Description { get; set; } = String.Empty;

        public string? Photo { get; set; }

        [Column(TypeName = "DATE")]
        public DateTime? ExpirationDate { get; set; }
    }
}