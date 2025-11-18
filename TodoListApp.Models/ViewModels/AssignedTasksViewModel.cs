namespace TodoListApp.Models.ViewModels;

public class AssignedTasksViewModel
{
    public List<AssignedTaskViewModel> Tasks { get; set; } = new();
    public Entities.Enums.StatusOfTask? CurrentFilter { get; set; }
    public string CurrentSort { get; set; } = "duedate";
    public int PageNumber { get; set; } = 1;
    public int TotalPages { get; set; }
    public int TotalCount { get; set; }
    public bool HasPreviousPage => PageNumber > 1;
    public bool HasNextPage => PageNumber < TotalPages;
}
