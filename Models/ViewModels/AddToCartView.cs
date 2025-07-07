using System.ComponentModel.DataAnnotations;

namespace EHSInventory.Models;

public class AddToCartView
{
    public string ProductName { get; set; } = String.Empty;
    public ProductUnit Unit { get; set; }

    [Required]
    public int Count { get; set; }
}