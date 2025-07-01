using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace EHSInventory.Models;

public class DeleteConfirmationView
{
    public string? Name { get; set; }

    [Required(ErrorMessage = "A comment explaining this delete is required.")]
    public string? Comment { get; set; }

    [BindNever]
    public long? CategoryId { get; set; }
}