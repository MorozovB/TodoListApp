namespace TodoListApp.Models.ViewModels;

public class TagViewModel
{
    public int Id { get; set; }
    public string Name { get; set; } = null!;
    public int TaskCount { get; set; }
}

public class TasksByTagViewModel
{
    public int TagId { get; set; }
    public string TagName { get; set; } = null!;
    public List<TaggedTaskViewModel> Tasks { get; set; } = new();
}

public class TaggedTaskViewModel
{
    public int Id { get; set; }
    public string Title { get; set; } = null!;
    public string? Description { get; set; }
    public bool IsCompleted { get; set; }
    public DateTime? DueDate { get; set; }
    public int TodoListId { get; set; }
    public bool IsOverdue { get; set; }

    public string FormattedDueDate => DueDate?.ToString("MMM dd, yyyy") ?? "No due date";
}
