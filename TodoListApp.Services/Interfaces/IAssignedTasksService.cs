using TodoListApp.Models.DTOs;
using TodoListApp.Models.Common;

namespace TodoListApp.Services.Interfaces;

public interface IAssignedTasksService
{
    Task<PagedResult<TodoTaskDto>> GetAssignedTasksAsync(string userId, Entities.Enums.StatusOfTask? statusFilter = null, string sortBy = "duedate", int pageNumber = 1, int pageSize = 10);
    Task<TodoTaskDto> ChangeStatusOfTaskAsync(int taskId, Entities.Enums.StatusOfTask newStatus, string userId);
    Task<TodoTaskDto> AssignTaskToUserAsync(int taskId, string newUserId, string currentUserId);
}
