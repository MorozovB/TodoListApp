using System.ComponentModel.DataAnnotations;

namespace TodoListApp.Models.ViewModels;

public class TodoTaskEditViewModel
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Task title is required")]
    [StringLength(200, ErrorMessage = "Title cannot exceed 200 characters")]
    [Display(Name = "Title")]
    public string Title { get; set; } = null!;

    [StringLength(1000, ErrorMessage = "Description cannot exceed 1000 characters")]
    [Display(Name = "Description")]
    [DataType(DataType.MultilineText)]
    public string? Description { get; set; }

    [Display(Name = "Due Date")]
    [DataType(DataType.Date)]
    public DateTime? DueDate { get; set; }

    [Required]
    [Display(Name = "Priority")]
    public int Priority { get; set; }

    public int TodoListId { get; set; }

    public string TodoListTitle { get; set; } = null!;
}
