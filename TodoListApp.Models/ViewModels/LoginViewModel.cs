using System.ComponentModel.DataAnnotations;

namespace TodoListApp.Models.ViewModels;
public class LoginViewModel
{
    [Required]
    [EmailAddress]
    public string? Email { get; set; }

    [Required]
    [DataType(DataType.Password)]
    [StringLength(25)]
    public string? Password { get; set; }

    public bool RememberMe { get; set; }
}
