namespace TodoListApp.Models.Common;
public class PagedResult<T> where T : class
{
    public List<T> Items { get; set; } = new List<T>();

    public int TotalCount { get; set; }

    public int PageNumber { get; set; }

    public int PageSize { get; set; }

    public int TotalPages => (int)Math.Ceiling((double)this.TotalCount / this.PageSize);

    public bool HasPreviousPage => this.PageNumber > 1;

    public bool HasNextPage => this.PageNumber < this.TotalPages;
}
