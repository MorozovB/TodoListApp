using TodoListApp.Models.Common;
using TodoListApp.Models.DTOs;

namespace TodoListApp.Services.Interfaces;

public interface ISearchService
{
    Task<PagedResult<TodoTaskDto>> SearchTasksAsync(string userId, string searchQuery, DateTime? startDate = null, DateTime? endDate = null, int pageNumber = 1, int pageSize = 10);
}
