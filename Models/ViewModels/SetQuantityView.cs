using System.ComponentModel.DataAnnotations;

namespace EHSInventory.Models;

public class SetQuantityView
{
    public long ProductId { get; set; }
    public string? ProductName { get; set; }

    public long? CategoryId { get; set; }
    public string CategoryName { get; set; } = String.Empty;

    [Required]
    public int NewQuantity { get; set; }

    [Required]
    public ProductUnit Unit { get; set; }

    [Required(ErrorMessage = "A comment is required explaining this change.")]
    public string? Comment { get; set; }
}