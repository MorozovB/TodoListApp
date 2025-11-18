using Microsoft.EntityFrameworkCore;
using TodoListApp.DataAccess.Context;
using TodoListApp.Models.Common;
using TodoListApp.Models.DTOs;
using TodoListApp.Services.Interfaces;

namespace TodoListApp.Services.Implementations.WebApi;

public class SearchDatabaseService : ISearchService
{
    private readonly TodoListDbContext _context;

    public SearchDatabaseService(TodoListDbContext context)
    {
        _context = context;
    }

    public async Task<PagedResult<TodoTaskDto>> SearchTasksAsync(
        string userId,
        string searchQuery,
        DateTime? startDate = null,
        DateTime? endDate = null,
        int pageNumber = 1,
        int pageSize = 10)
    {
        var query = _context.Tasks
            .Include(t => t.TodoList)
            .Where(t => t.TodoList!.OwnerId == userId || t.AssignedToUserId == userId);

        if (!string.IsNullOrWhiteSpace(searchQuery))
        {
            searchQuery = searchQuery.ToLower();
            query = query.Where(t => t.Title!.ToLower().Contains(searchQuery) ||
                                    (t.Description != null && t.Description.ToLower().Contains(searchQuery)));
        }

        if (startDate.HasValue)
        {
            query = query.Where(t => t.CreatedDate >= startDate.Value ||
                                    (t.DueDate.HasValue && t.DueDate >= startDate.Value));
        }

        if (endDate.HasValue)
        {
            query = query.Where(t => t.CreatedDate <= endDate.Value ||
                                    (t.DueDate.HasValue && t.DueDate <= endDate.Value));
        }

        var totalCount = await query.CountAsync();

        var tasks = await query
            .OrderBy(t => t.DueDate ?? DateTime.MaxValue)
            .ThenBy(t => t.Title)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .Select(t => new TodoTaskDto
            {
                Id = t.Id,
                Title = t.Title!,
                Description = t.Description,
                IsCompleted = t.IsCompleted,
                Priority = t.Priority,
                Status = t.Status,
                CreatedDate = t.CreatedDate,
                CompletedDate = t.CompletedDate,
                DueDate = t.DueDate,
                TodoListId = t.TodoListId,
                AssignedToUserId = t.AssignedToUserId
            })
            .ToListAsync();

        return new PagedResult<TodoTaskDto>
        {
            Items = tasks,
            TotalCount = totalCount,
            PageNumber = pageNumber,
            PageSize = pageSize
        };
    }
}
