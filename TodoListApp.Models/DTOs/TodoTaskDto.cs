using TodoListApp.Entities.Enums;

namespace TodoListApp.Models.DTOs;
public class TodoTaskDto
{
    public int Id { get; set; }

    public string Title { get; set; } = null!;

    public string? Description { get; set; }

    public bool IsCompleted { get; set; }

    public TaskPriority Priority { get; set; }

    public bool Status { get; set; }

    public DateTime CreatedDate { get; set; }

    public DateTime? CompletedDate { get; set; }

    public DateTime DueDate { get; set; }

    public int TodoListId { get; set; }
}
