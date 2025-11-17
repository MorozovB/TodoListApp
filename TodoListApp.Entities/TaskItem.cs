using TodoListApp.Entities.Enums;

namespace TodoListApp.Entities;
public class TaskItem
{
    public int Id { get; set; }

    public string? Title { get; set; }

    public string? Description { get; set; }

    public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

    public bool IsCompleted { get; set; }

    public DateTime? CompletedDate { get; set; }

    public DateTime? DueDate { get; set; }

    public TaskPriority Priority { get; set; } = TaskPriority.Medium;

    public bool Status { get; set; }

    public int TodoListId { get; set; }

    public TodoListEntity? TodoList { get; set; }
}
