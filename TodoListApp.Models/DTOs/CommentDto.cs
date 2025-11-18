namespace TodoListApp.Models.DTOs;

public class CommentDto
{
    public int Id { get; set; }
    public int TaskId { get; set; }
    public string Content { get; set; } = null!;
    public DateTime CreatedDate { get; set; }
    public string? CreatedBy { get; set; }
}
