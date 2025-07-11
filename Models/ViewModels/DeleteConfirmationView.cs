using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace EHSInventory.Models;

public class DeleteConfirmationView
{
    public string? Name { get; set; }

    [Required(ErrorMessage = "A comment explaining this delete is required.")]
    public string? Comment { get; set; }

    public long? CategoryId { get; set; }

    public string CategoryName { get; set; } = String.Empty;  

    public long? ProductId { get; set; }
}