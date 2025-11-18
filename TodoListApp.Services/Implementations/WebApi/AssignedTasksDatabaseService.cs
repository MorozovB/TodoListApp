using Microsoft.EntityFrameworkCore;
using TodoListApp.DataAccess.Context;
using TodoListApp.Models.Common;
using TodoListApp.Models.DTOs;
using TodoListApp.Services.Interfaces;

namespace TodoListApp.Services.Implementations.WebApi;

public class AssignedTasksDatabaseService : IAssignedTasksService
{
    private readonly TodoListDbContext _context;

    public AssignedTasksDatabaseService(TodoListDbContext context)
    {
        _context = context;
    }

    public async Task<PagedResult<TodoTaskDto>> GetAssignedTasksAsync(
        string userId,
        Entities.Enums.StatusOfTask? statusFilter = null,
        string sortBy = "duedate",
        int pageNumber = 1,
        int pageSize = 10)
    {
        var query = _context.Tasks
            .Where(t => t.AssignedToUserId == userId);

        if (statusFilter.HasValue)
        {
            query = query.Where(t => t.Status == statusFilter.Value);
        }
        else
        {
            query = query.Where(t => t.Status != Entities.Enums.StatusOfTask.Completed);
        }

        query = sortBy?.ToLower() switch
        {
            "name" => query.OrderBy(t => t.Title),
            "duedate" => query.OrderBy(t => t.DueDate ?? DateTime.MaxValue),
            "priority" => query.OrderByDescending(t => t.Priority),
            "status" => query.OrderBy(t => t.Status),
            _ => query.OrderBy(t => t.DueDate ?? DateTime.MaxValue)
        };

        var totalCount = await query.CountAsync();

        var tasks = await query
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

    public async Task<TodoTaskDto> ChangeStatusOfTaskAsync(int taskId, Entities.Enums.StatusOfTask newStatus, string userId)
    {
        var task = await _context.Tasks.FindAsync(taskId);

        if (task == null)
        {
            throw new KeyNotFoundException($"Task with ID {taskId} not found");
        }

        if (task.AssignedToUserId != userId)
        {
            throw new UnauthorizedAccessException("You can only change status of tasks assigned to you");
        }

        task.Status = newStatus;

        if (newStatus == Entities.Enums.StatusOfTask.Completed)
        {
            task.IsCompleted = true;
            task.CompletedDate = DateTime.UtcNow;
        }
        else
        {
            task.IsCompleted = false;
            task.CompletedDate = null;
        }

        await _context.SaveChangesAsync();

        return new TodoTaskDto
        {
            Id = task.Id,
            Title = task.Title!,
            Description = task.Description,
            IsCompleted = task.IsCompleted,
            Priority = task.Priority,
            Status = task.Status,
            CreatedDate = task.CreatedDate,
            CompletedDate = task.CompletedDate,
            DueDate = task.DueDate,
            TodoListId = task.TodoListId,
            AssignedToUserId = task.AssignedToUserId
        };
    }

    public async Task<TodoTaskDto> AssignTaskToUserAsync(int taskId, string newUserId, string currentUserId)
    {
        var task = await _context.Tasks
            .Include(t => t.TodoList)
            .FirstOrDefaultAsync(t => t.Id == taskId);

        if (task == null)
        {
            throw new KeyNotFoundException($"Task with ID {taskId} not found");
        }

        if (task.TodoList!.OwnerId != currentUserId)
        {
            throw new UnauthorizedAccessException("Only list owner can reassign tasks");
        }

        task.AssignedToUserId = newUserId;
        await _context.SaveChangesAsync();

        return new TodoTaskDto
        {
            Id = task.Id,
            Title = task.Title!,
            Description = task.Description,
            IsCompleted = task.IsCompleted,
            Priority = task.Priority,
            Status = task.Status,
            CreatedDate = task.CreatedDate,
            CompletedDate = task.CompletedDate,
            DueDate = task.DueDate,
            TodoListId = task.TodoListId,
            AssignedToUserId = task.AssignedToUserId
        };
    }
}
