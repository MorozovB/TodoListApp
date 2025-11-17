using TodoListApp.Models.DTOs;
using TodoListApp.Models.Common;

namespace TodoListApp.Services.Interfaces
{
    public interface ITodoListService
    {
        Task<PagedResult<TodoListDto>> GetUserTodoListsAsync(string userId, int pageNumber, int pageSize);
        Task<TodoListDto?> GetTodoListByIdAsync(int id, string userId);
        Task<IEnumerable<TodoTaskDto>> GetTasksByIdAsync(int id, string userId);
        Task<TodoListDto> CreateAsync(TodoListDto todoListDto, string userId);
        Task UpdateAsync(TodoListDto todoListDto, string userId);
        Task DeleteAsync(int id, string userId);
    }
}
