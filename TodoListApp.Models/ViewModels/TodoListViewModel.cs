namespace TodoListApp.Models.ViewModels;
public class TodoListViewModel
{
    public int Id { get; set; }

    public string Title { get; set; } = null!;

    public string? Description { get; set; }

    public DateTime CreatedDate { get; set; }

    public int TaskCount { get; set; }

    public string FormattedCreateDate => this.CreatedDate.ToString("MMMM dd, yyyy");
}
