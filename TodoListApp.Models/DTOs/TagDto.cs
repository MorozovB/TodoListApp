namespace TodoListApp.Models.DTOs;

public class TagDto
{
    public int Id { get; set; }
    public string Name { get; set; } = null!;
    public int TaskCount { get; set; }
}
