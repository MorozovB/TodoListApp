namespace TodoListApp.Models.ViewModels;

public class TodoTaskViewModel
{
    public int Id { get; set; }

    public string Title { get; set; } = null!;

    public string? Description { get; set; }

    public bool IsCompleted { get; set; }

    public DateTime? DueDate { get; set; }

    public string FormattedDueDate
        => DueDate?.ToString("MMMM dd, yyyy") ?? "No due date";

    public int Priority { get; set; }

    public DateTime CreatedDate { get; set; }

    public string? PriorityCssClass { get; set; }

    public bool IsOverdue => DueDate.HasValue &&
                             DueDate.Value.Date < DateTime.UtcNow.Date &&
                             !IsCompleted;

    public string RowCssClass => IsOverdue ? "table-danger" : "";

    public string DueDateCssClass => IsOverdue ? "text-danger fw-bold" : "";
}
