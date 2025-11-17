namespace TodoListApp.Models.DTOs
{
    public class TodoListDto
    {
        public int Id { get; set; }

        public string Title { get; set; } = string.Empty;

        public string? Description { get; set; }

        public DateTime CreatedDate { get; set; }

        public int TaskCount { get; set; }

        public string? OwnerId { get; set; }
    }
}
