using TodoListApp.Models.DTOs;

namespace TodoListApp.Services.Interfaces;
public interface ITodoTaskService
{
    Task<IEnumerable<TodoTaskDto>> GetTasksByListIdAsync(int listId, string userId);
    Task<TodoTaskDto> GetByIdAsync(int taskId, string userId);
    Task<TodoTaskDto> CreateAsync(TodoTaskDto dto, int listId, string userId);
    Task<TodoTaskDto> UpdateAsync(TodoTaskDto dto, string userId);
    Task DeleteAsync(int taskId, string userId);
    Task<TodoTaskDto> ToggleCompletedAsync(int taskId, string userId);
}
