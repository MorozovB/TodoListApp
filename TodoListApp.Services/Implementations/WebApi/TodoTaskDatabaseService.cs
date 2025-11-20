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

    public Task<IEnumerable<TodoTaskDto>> GetTasksByListIdAsync(int listId, string userId)
    {
        if (string.IsNullOrEmpty(userId))
        {
            throw new ArgumentException("User ID cannot be null or empty.", nameof(userId));
        }
        if (listId <= 0)
        {
            throw new ArgumentException("List ID must be greater than zero.", nameof(listId));
        }
        return GetTasksByListIdAsyncCore(listId, userId);
    }

    private async Task<IEnumerable<TodoTaskDto>> GetTasksByListIdAsyncCore(int listId, string userId)
    {
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

    public Task<TodoTaskDto> GetByIdAsync(int taskId, string userId)
    {
        if (taskId <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(taskId));
        }
        ArgumentNullException.ThrowIfNull(userId);
        return GetByIdAsyncCore(taskId, userId);
    }

    private async Task<TodoTaskDto> GetByIdAsyncCore(int taskId, string userId)
    {
        var taskEntity = await _taskRepository.GetByIdAsync(taskId);
        if (taskEntity == null)
        {
            throw new KeyNotFoundException($"Task with ID {taskId} not found");
        }

        var isOwner = await _listRepository.IsOwnerAsync(taskEntity.TodoListId, userId);
        if (!isOwner)
        {
            throw new UnauthorizedAccessException("User is not the owner of the todo list.");
        }
        return new TodoTaskDto
        {
            Id = taskEntity.Id,
            Title = taskEntity.Title!,
            Description = taskEntity.Description,
            Priority = taskEntity.Priority,
            Status = taskEntity.Status,
            DueDate = taskEntity.DueDate,
            CreatedDate = taskEntity.CreatedDate,
            IsCompleted = taskEntity.IsCompleted,
            CompletedDate = taskEntity.CompletedDate,
            TodoListId = taskEntity.TodoListId,
            AssignedToUserId = taskEntity.AssignedToUserId
        };
    }

    public Task<TodoTaskDto> CreateAsync(TodoTaskDto dto, int listId, string userId)
    {
        ArgumentNullException.ThrowIfNull(dto);

        if (listId <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(listId));
        }
        if (string.IsNullOrEmpty(userId))
        {
            throw new ArgumentException("User ID cannot be null or empty.", nameof(userId));
        }
        return CreateAsyncCore(dto, listId, userId);
    }

    private async Task<TodoTaskDto> CreateAsyncCore(TodoTaskDto dto, int listId, string userId)
    {
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
            CreatedDate = DateTime.UtcNow,
            DueDate = dto.DueDate,
            AssignedToUserId = userId
        };
        var createdTask = await this._taskRepository.AddAsync(taskEntity);
        return new TodoTaskDto
        {
            Id = createdTask.Id,
            Title = createdTask.Title!,
            Description = createdTask.Description,
            Priority = createdTask.Priority,
            Status = createdTask.Status,
            CreatedDate = createdTask.CreatedDate,
            DueDate = createdTask.DueDate,
            TodoListId = createdTask.TodoListId,
            AssignedToUserId = createdTask.AssignedToUserId
        };
    }

    public Task<TodoTaskDto> UpdateAsync(TodoTaskDto dto, string userId)
    {
        ArgumentNullException.ThrowIfNull(dto);
        if (string.IsNullOrEmpty(userId))
        {
            throw new ArgumentException("User ID cannot be null or empty.", nameof(userId));
        }
        return UpdateAsyncCore(dto, userId);
    }

    private async Task<TodoTaskDto> UpdateAsyncCore(TodoTaskDto dto, string userId)
    {
        var taskEntity = await _taskRepository.GetByIdAsync(dto.Id);
        if (taskEntity == null)
        {
            throw new KeyNotFoundException($"Task with ID {dto.Id} not found");
        }
        var isOwner = await _listRepository.IsOwnerAsync(taskEntity.TodoListId, userId);
        if (!isOwner)
        {
            throw new UnauthorizedAccessException("User is not the owner of the todo list.");
        }
        taskEntity.Title = dto.Title;
        taskEntity.Description = dto.Description;
        taskEntity.Priority = dto.Priority;
        taskEntity.Status = dto.Status;
        taskEntity.DueDate = dto.DueDate;
        var updatedTask = await _taskRepository.UpdateAsync(taskEntity);
        return new TodoTaskDto
        {
            Id = updatedTask.Id,
            Title = updatedTask.Title!,
            Description = updatedTask.Description,
            Priority = updatedTask.Priority,
            Status = updatedTask.Status,
            CreatedDate = updatedTask.CreatedDate,
            DueDate = updatedTask.DueDate,
            IsCompleted = updatedTask.IsCompleted,
            CompletedDate = updatedTask.CompletedDate,
            TodoListId = updatedTask.TodoListId,
            AssignedToUserId = updatedTask.AssignedToUserId
        };
    }

    public Task DeleteAsync(int taskId, string userId)
    {
        if (taskId <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(taskId));
        }
        if (string.IsNullOrEmpty(userId))
        {
            throw new ArgumentException("User ID cannot be null or empty.", nameof(userId));
        }
        return DeleteAsyncCore(taskId, userId);
    }

    private async Task DeleteAsyncCore(int taskId, string userId)
    {
        var taskEntity = await _taskRepository.GetByIdAsync(taskId);
        if (taskEntity == null)
        {
            throw new KeyNotFoundException($"Task with ID {taskId} not found");
        }
        var isOwner = await _listRepository.IsOwnerAsync(taskEntity.TodoListId, userId);
        if (!isOwner)
        {
            throw new UnauthorizedAccessException("User is not the owner of the todo list.");
        }
        await _taskRepository.DeleteAsync(taskId);
    }

    public Task<TodoTaskDto> ToggleCompletedAsync(int taskId, string userId)
    {
        if (taskId <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(taskId));
        }
        if (string.IsNullOrEmpty(userId))
        {
            throw new ArgumentException("User ID cannot be null or empty.", nameof(userId));
        }
        return ToggleCompletedAsyncCore(taskId, userId);
    }

    private async Task<TodoTaskDto> ToggleCompletedAsyncCore(int taskId, string userId)
    {
        var taskEntity = await _taskRepository.GetByIdAsync(taskId);
        if (taskEntity == null)
        {
            throw new KeyNotFoundException($"Task with ID {taskId} not found");
        }

        var isOwner = await _listRepository.IsOwnerAsync(taskEntity.TodoListId, userId);
        if (!isOwner)
        {
            throw new UnauthorizedAccessException("User is not the owner of the todo list.");
        }
        taskEntity.IsCompleted = !taskEntity.IsCompleted;
        if (taskEntity.IsCompleted)
        {
            taskEntity.CompletedDate = DateTime.UtcNow;
            taskEntity.Status = Entities.Enums.StatusOfTask.Completed;
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
            DueDate = taskEntity.DueDate,
            Priority = taskEntity.Priority,
            Status = taskEntity.Status,
            TodoListId = taskEntity.TodoListId,
            AssignedToUserId = taskEntity.AssignedToUserId
        };
    }
}
