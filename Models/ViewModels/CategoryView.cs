using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EHSInventory.Models
{
    public class CategoryView
    {
        public List<ProductCategory> AllCategories { get; set; } = new List<ProductCategory>();

        public ProductCategory? CurrentCategory { get; set; }

        public List<Product> Products { get; set; } = new List<Product>();

        public int ItemCount { get; set; }
    }
}