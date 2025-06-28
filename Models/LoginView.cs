using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;

namespace EHSInventory.Models;

public class LoginView
{
    [BindProperty]
    [Required(ErrorMessage = "A username is required.")]
    public string Username { get; set; } = string.Empty;

    [BindProperty]
    [Required(ErrorMessage = "A password is required.")]
    [DataType(DataType.Password)]
    public string Password { get; set; } = string.Empty;
}