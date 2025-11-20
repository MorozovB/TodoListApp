namespace TodoListApp.Entities
{
    public class TodoListEntity
    {
        public int Id { get; set; }

        public string? Title { get; set; }

        public string? Description { get; set; }

        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

        public string? OwnerId { get; set; }

        public ICollection<TaskItem> Tasks { get; set; } = null!;

    }
}
