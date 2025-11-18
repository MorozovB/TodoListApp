namespace TodoListApp.Entities;

public class Comment
{
    public int Id { get; set; }

    public int TaskId { get; set; }

    public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

    public string? CreatedBy { get; set; }

    public TaskItem? Task { get; set; }

    public string Content { get; set; } = string.Empty;
}
