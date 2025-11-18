using TodoListApp.Entities.Enums;

namespace TodoListApp.Models.ViewModels;

public class AssignedTaskViewModel
{
    public int Id { get; set; }
    public string Title { get; set; } = null!;
    public string? Description { get; set; }
    public StatusOfTask Status { get; set; }
    public TaskPriority Priority { get; set; }
    public DateTime? DueDate { get; set; }
    public bool IsOverdue { get; set; }
    public int TodoListId { get; set; }

    public string StatusText => Status switch
    {
        StatusOfTask.NotStarted => "Not Started",
        StatusOfTask.InProgress => "In Progress",
        StatusOfTask.Completed => "Completed",
        _ => "Unknown"
    };

    public string StatusBadgeClass => Status switch
    {
        StatusOfTask.NotStarted => "badge bg-secondary",
        StatusOfTask.InProgress => "badge bg-primary",
        StatusOfTask.Completed => "badge bg-success",
        _ => "badge bg-secondary"
    };

    public string PriorityText => Priority switch
    {
        TaskPriority.Low => "Low",
        TaskPriority.Medium => "Medium",
        TaskPriority.High => "High",
        _ => "Unknown"
    };

    public string PriorityBadgeClass => Priority switch
    {
        TaskPriority.Low => "badge bg-info",
        TaskPriority.Medium => "badge bg-warning text-dark",
        TaskPriority.High => "badge bg-danger",
        _ => "badge bg-secondary"
    };

    public string FormattedDueDate => DueDate?.ToString("MMM dd, yyyy") ?? "No due date";
}
