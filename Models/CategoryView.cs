using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EHSInventory.Models
{
    public class CategoryView
    {
        public List<ProductCategory> AllCategories { get; set; } = new List<ProductCategory>();

        public ProductCategory? CurrentCategory;

        public List<Product> Products { get; set; } = new List<Product>();
    }
}