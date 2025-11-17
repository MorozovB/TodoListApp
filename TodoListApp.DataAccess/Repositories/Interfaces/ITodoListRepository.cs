using TodoListApp.Entities;

namespace TodoListApp.DataAccess.Repositories.Interfaces;
public interface ITodoListRepository
{
    Task<IEnumerable<TodoListEntity>> GetAllByOwnerIdAsync(string ownerId);
    Task<TodoListEntity?> GetByIdAsync(int id);
    Task<TodoListEntity> CreateAsync(TodoListEntity todoList);
    Task UpdateSync(TodoListEntity todoList);
    Task DeleteAsync(int id);
    Task<bool> ExistsAsync(int id);
    Task<bool> IsOwnerAsync(int todoListId, string userId);
}
