namespace TodoListApp.Models.ViewModels;
public class TodoListDetailsViewModel
{
    public int Id { get; set; }

    public string Title { get; set; } = null!;

    public string? Description { get; set; }

    public DateTime CreatedDate { get; set; }

    public string FormattedCreatedDate
        => CreatedDate.ToString("MMMM dd, yyyy");

    public List<TodoTaskViewModel> Tasks { get; set; } = new List<TodoTaskViewModel>();

    public int CompletedTasksCount { get; set; }

    public int TotalTasksCount { get; set; }

    public bool CanEdit { get; set; }

    public bool CanDelete { get; set; }

    public bool HasTasks { get; set; }
}
