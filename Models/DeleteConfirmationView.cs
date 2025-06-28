using System.ComponentModel.DataAnnotations;

namespace EHSInventory.Models;

public class DeleteConfirmationView
{
    public string? Name { get; set; }
    
    [Required(ErrorMessage = "A comment explaining this delete is required.")]
    public string? Comment { get; set; }
}