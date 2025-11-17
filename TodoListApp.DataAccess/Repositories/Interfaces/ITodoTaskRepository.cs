using TodoListApp.Entities;

namespace TodoListApp.DataAccess.Repositories.Interfaces;
public interface ITodoTaskRepository
{
    Task<TaskItem?> GetByIdAsync(int id);
    Task<List<TaskItem>> GetByListIdAsync(int listId);
    Task<IEnumerable<TaskItem>> GetByListIdAsync(int listId, bool? isComleted);
    Task<TaskItem> AddAsync(TaskItem item);
    Task<TaskItem> UpdateAsync(TaskItem item);
    Task DeleteAsync(int id);
    Task<int?> CountByListIdAsync(int listId);
}
