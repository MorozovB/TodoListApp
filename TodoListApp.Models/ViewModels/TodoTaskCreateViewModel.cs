using System.ComponentModel.DataAnnotations;

namespace TodoListApp.Models.ViewModels;
public class TodoTaskCreateViewModel
{
    [Required]
    [MaxLength(200)]
    public string Title { get; set; } = null!;

    [MaxLength(1000)]
    public string? Description { get; set; }

    [Display(Name = "Due Date")]
    public DateTime? DueDate { get; set; }
    public int Priority { get; set; }
}
