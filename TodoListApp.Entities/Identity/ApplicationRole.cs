using Microsoft.AspNetCore.Identity;

namespace TodoListApp.Entities.Identity;
public class ApplicationRole : IdentityRole
{
    public string? Description { get; set; }

    public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

    public bool IsSystemRole { get; set; } = false;

    public bool IsActive { get; set; } = true;
}
