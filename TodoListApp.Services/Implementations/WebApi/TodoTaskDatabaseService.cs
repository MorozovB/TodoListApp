using TodoListApp.DataAccess.Repositories.Interfaces;
using TodoListApp.Entities;
using TodoListApp.Models.DTOs;
using TodoListApp.Services.Interfaces;

namespace TodoListApp.Services.Implementations.WebApi;
public class TodoTaskDatabaseService : ITodoTaskService
{
    private readonly ITodoTaskRepository _taskRepository;
    private readonly ITodoListRepository _listRepository;

    public TodoTaskDatabaseService(
        ITodoTaskRepository taskRepository,
        ITodoListRepository listRepository)
    {
        _taskRepository = taskRepository;
        _listRepository = listRepository;
    }

    public async Task<IEnumerable<TodoTaskDto>> GetTasksByListIdAsync(int listId, string userId)
    {
        if (string.IsNullOrEmpty(userId))
        {
            throw new ArgumentException("User ID cannot be null or empty.", nameof(userId));
        }

        if (listId <= 0)
        {
            throw new ArgumentException("List ID must be greater than zero.", nameof(listId));
        }

        var listExists = await _listRepository.ExistsAsync(listId);
        if (!listExists)
        {
            throw new KeyNotFoundException($"Todo list with ID {listId} was not found.");
        }

        var isOwner = await _listRepository.IsOwnerAsync(listId, userId);
        if (!isOwner)
        {
            throw new UnauthorizedAccessException(
                $"You don't have permission to access tasks for todo list {listId}.");
        }

        var tasks = await _taskRepository.GetByListIdAsync(listId);

        var taskDtos = tasks.Select(task => new TodoTaskDto
        {
            Id = task.Id,
            Title = task.Title!,
            Description = task.Description,
            Priority = task.Priority,
            Status = task.Status,
            TodoListId = task.TodoListId
        }).ToList();

        return taskDtos;
    }

    public async Task<TodoTaskDto> GetByIdAsync(int taskId, string userId)
    {
        ArgumentNullException.ThrowIfNull(taskId);
        ArgumentNullException.ThrowIfNull(userId);

        var isOwner = await _listRepository.IsOwnerAsync(taskId, userId);

        if (!isOwner)
        {
            throw new UnauthorizedAccessException("User is not the owner of the todo list.");
        }

        var taskEntity = await _taskRepository.GetByIdAsync(taskId);

        return new TodoTaskDto
        {
            Id = taskEntity.Id,
            Title = taskEntity.Title!,
            Description = taskEntity.Description,
            Priority = taskEntity.Priority,
            Status = taskEntity.Status,
        };
    }

    public async Task<TodoTaskDto> CreateAsync(TodoTaskDto dto, int listId, string userId)
    {
        ArgumentNullException.ThrowIfNull(dto);
        ArgumentNullException.ThrowIfNull(listId);
        ArgumentNullException.ThrowIfNull(userId);

        var listExists = await _listRepository.ExistsAsync(listId);
        if (!listExists)
        {
            throw new KeyNotFoundException($"Todo list with ID {listId} was not found.");
        }

        var isOwner = await _listRepository.IsOwnerAsync(listId, userId);
        if (!isOwner)
        {
            throw new UnauthorizedAccessException(
                $"You don't have permission to access tasks for todo list {listId}.");
        }

        var taskEntity = new TaskItem
        {
            TodoListId = listId,
            Title = dto.Title,
            Description = dto.Description,
            Priority = dto.Priority,
            Status = dto.Status,
        };
        taskEntity.CreatedDate = DateTime.UtcNow;

        var createdTask = await this._taskRepository.AddAsync(taskEntity);

        return new TodoTaskDto
        {
            Id = createdTask.Id,
            Title = createdTask.Title!,
            Description = createdTask.Description,
            Priority = createdTask.Priority,
            Status = createdTask.Status,
            CreatedDate = createdTask.CreatedDate,
        };
    }

    public async Task<TodoTaskDto> UpdateAsync(TodoTaskDto dto, string userId)
    {
        ArgumentNullException.ThrowIfNull(dto);
        ArgumentNullException.ThrowIfNull(userId);

        var isOwner = await _listRepository.IsOwnerAsync(dto.Id, userId);

        if (!isOwner)
        {
            throw new UnauthorizedAccessException("User is not the owner of the todo list.");
        }

        var taskEntity = await _taskRepository.GetByIdAsync(dto.Id);

        taskEntity.Title = dto.Title;
        taskEntity.Description = dto.Description;
        taskEntity.Priority = dto.Priority;
        taskEntity.Status = dto.Status;
        taskEntity.CreatedDate = DateTime.UtcNow;

        var updatedTask = await _taskRepository.UpdateAsync(taskEntity);

        return new TodoTaskDto
        {
            Title = updatedTask.Title!,
            Description = updatedTask.Description,
            Priority = updatedTask.Priority,
            Status = updatedTask.Status,
            CreatedDate = updatedTask.CreatedDate,
        };

    }

    public async Task DeleteAsync(int taskId, string userId)
    {
        ArgumentNullException.ThrowIfNull(taskId);
        ArgumentNullException.ThrowIfNull(userId);

        var isOwner = await _listRepository.IsOwnerAsync(taskId, userId);

        if (!isOwner)
        {
            throw new UnauthorizedAccessException("User is not the owner of the todo list.");
        }

        await _taskRepository.DeleteAsync(taskId);

    }

    public async Task<TodoTaskDto> ToggleCompletedAsync(int taskId, string userId)
    {
        ArgumentNullException.ThrowIfNull(taskId);
        ArgumentNullException.ThrowIfNull(userId);

        var isOwner = await _listRepository.IsOwnerAsync(taskId, userId);

        if (!isOwner)
        {
            throw new UnauthorizedAccessException("User is not the owner of the todo list.");
        }

        var taskEntity = await _taskRepository.GetByIdAsync(taskId);

        taskEntity.IsCompleted = !taskEntity.IsCompleted;

        if (taskEntity.IsCompleted)
        {
            taskEntity.CompletedDate = DateTime.UtcNow;
        }
        else
        {
            taskEntity.CompletedDate = null;
        }

        await _taskRepository.UpdateAsync(taskEntity);

        return new TodoTaskDto
        {
            Id = taskEntity.Id,
            Title = taskEntity.Title!,
            Description = taskEntity.Description,
            IsCompleted = taskEntity.IsCompleted,
            CompletedDate = taskEntity.CompletedDate,
            CreatedDate = taskEntity.CreatedDate,
            TodoListId = taskEntity.TodoListId,
        };

    }
}
