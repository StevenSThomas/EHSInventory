using System.ComponentModel.DataAnnotations;

namespace EHSInventory.Models;

public class SetQuantityView
{
    public long ProductId { get; set; }
    public string? ProductName { get; set; }

    [Required]
    public int NewQuantity { get; set; }

    [Required(ErrorMessage = "A comment is required explaining this change.")]
    public string? Comment { get; set; }
}