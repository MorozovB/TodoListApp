namespace TodoListApp.Models.ViewModels;

public class TodoTaskDetailsViewModel
{
    public int Id { get; set; }

    public string Title { get; set; } = null!;

    public string? Description { get; set; }

    public bool IsCompleted { get; set; }

    public int Priority { get; set; }

    public DateTime CreatedDate { get; set; }

    public DateTime? CompletedDate { get; set; }

    public DateTime? DueDate { get; set; }

    public int TodoListId { get; set; }

    public string TodoListTitle { get; set; } = null!;

    public bool IsOverdue { get; set; }

    public string FormattedCreatedDate
        => CreatedDate.ToString("MMMM dd, yyyy");

    public string FormattedCompletedDate
        => CompletedDate?.ToString("MMMM dd, yyyy") ?? "Not completed";

    public string FormattedDueDate
        => DueDate?.ToString("MMMM dd, yyyy") ?? "No due date";

    public string PriorityText => Priority switch
    {
        0 => "Low",
        1 => "Medium",
        2 => "High",
        _ => "Unknown"
    };

    public string PriorityCssClass => Priority switch
    {
        0 => "badge bg-secondary",
        1 => "badge bg-warning text-dark",
        2 => "badge bg-danger",
        _ => "badge bg-secondary"
    };

    public string StatusBadgeClass
    {
        get
        {
            if (IsCompleted)
            {
                return "badge bg-success";
            }
            else if (IsOverdue)
            {
                return "badge bg-danger";
            }
            else
            {
                return "badge bg-primary";
            }
        }
    }

    public string StatusText
    {
        get
        {
            if (IsCompleted)
            {
                return "Completed";
            }
            else if (IsOverdue)
            {
                return "Overdue";
            }
            else
            {
                return "Active";
            }
        }
    }
}
