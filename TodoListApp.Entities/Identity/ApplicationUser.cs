using Microsoft.AspNetCore.Identity;

namespace TodoListApp.Entities.Identity;

public class ApplicationUser : IdentityUser
{
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public DateTime? DateOfBirth { get; set; }
    public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
    public DateTime? LastLoginDate { get; set; }

    public string FullName => $"{FirstName} {LastName}".Trim();
}
