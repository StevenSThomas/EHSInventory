using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

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

    public class Product
    {
        public long? ProductId { get; set; }

        public ProductCategory? Category { get; set; }

        [Required]
        [Column(TypeName = "VARCHAR")]
        [StringLength(255)]
        public string? Name { get; set; }

        public ProductUnit Unit { get; set; } = ProductUnit.Individual;

        public int Quantity { get; set; } = 0;

        public int DisplayOrder { get; set; } = 0;

        [Column(TypeName = "VARCHAR")]
        [StringLength(50)]
        public string? GrangerNum { get; set; }

        [Column(TypeName = "TEXT")]
        public string Description { get; set; } = String.Empty;

        public string? Photo { get; set; }

        [Column(TypeName = "DATE")]
        public DateTime? ExpirationDate { get; set; }

    }
}