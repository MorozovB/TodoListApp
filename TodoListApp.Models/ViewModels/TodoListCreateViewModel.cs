using System.ComponentModel.DataAnnotations;

namespace TodoListApp.Models.ViewModels;
public class TodoListCreateViewModel
{
    [Required]
    [StringLength(100)]
    public string? Title { get; set; }

    [StringLength(1000)]
    [DataType(DataType.MultilineText)]
    public string? Description { get; set; }
}
