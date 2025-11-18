using TodoListApp.Entities.Enums;

namespace TodoListApp.Models.ViewModels;

public class SearchViewModel
{
    public string? SearchQuery { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
}

public class SearchResultsViewModel
{
    public string SearchQuery { get; set; } = "";
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public List<SearchTaskViewModel> Tasks { get; set; } = new();
    public int PageNumber { get; set; } = 1;
    public int TotalPages { get; set; }
    public int TotalCount { get; set; }
    public bool HasPreviousPage => PageNumber > 1;
    public bool HasNextPage => PageNumber < TotalPages;
}

public class SearchTaskViewModel
{
    public int Id { get; set; }
    public string Title { get; set; } = null!;
    public string? Description { get; set; }
    public StatusOfTask Status { get; set; }
    public TaskPriority Priority { get; set; }
    public DateTime? DueDate { get; set; }
    public DateTime CreatedDate { get; set; }
    public bool IsCompleted { get; set; }
    public bool IsOverdue { get; set; }
    public int TodoListId { get; set; }

    public string StatusBadgeClass => Status switch
    {
        StatusOfTask.NotStarted => "badge bg-secondary",
        StatusOfTask.InProgress => "badge bg-primary",
        StatusOfTask.Completed => "badge bg-success",
        _ => "badge bg-secondary"
    };

    public string PriorityBadgeClass => Priority switch
    {
        TaskPriority.Low => "badge bg-info",
        TaskPriority.Medium => "badge bg-warning text-dark",
        TaskPriority.High => "badge bg-danger",
        _ => "badge bg-secondary"
    };
}
