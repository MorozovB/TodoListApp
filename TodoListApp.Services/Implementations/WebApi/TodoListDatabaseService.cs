using TodoListApp.DataAccess.Repositories.Interfaces;
using TodoListApp.Entities;
using TodoListApp.Models.Common;
using TodoListApp.Models.DTOs;
using TodoListApp.Services.Interfaces;

namespace TodoListApp.Services.Implementations.WebApi;
public class TodoListDatabaseService : ITodoListService
{
    private readonly ITodoListRepository _todoListRepository;
    private readonly ITodoTaskRepository _todoTaskRepository;

    public TodoListDatabaseService(ITodoListRepository todoListRepository, ITodoTaskRepository todoTaskRepository)
    {
        this._todoListRepository = todoListRepository;
        this._todoTaskRepository = todoTaskRepository;
    }

    public Task<PagedResult<TodoListDto>> GetUserTodoListsAsync(string userId, int pageNumber, int pageSize)
    {
        if (pageNumber <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(pageNumber));
        }
        if (pageSize <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(pageSize));
        }
        return GetUserTodoListsAsyncCore(userId, pageNumber, pageSize);
    }

    private async Task<PagedResult<TodoListDto>> GetUserTodoListsAsyncCore(string userId, int pageNumber, int pageSize)
    {
        var allLists = await this._todoListRepository.GetAllByOwnerIdAsync(userId);
        var pagedLists = allLists
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToList();
        var dtoLists = pagedLists
            .Select(t => new TodoListDto
            {
                Id = t.Id,
                Title = t.Title!,
                Description = t.Description,
                CreatedDate = t.CreatedDate,
                TaskCount = t.Tasks?.Count ?? 0,
            }).ToList();
        return new PagedResult<TodoListDto>
        {
            Items = dtoLists,
            TotalCount = allLists.Count(),
            PageNumber = pageNumber,
            PageSize = pageSize,
        };
    }

    public Task<TodoListDto?> GetTodoListByIdAsync(int id, string userId)
    {
        if (id <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(id));
        }
        return GetTodoListByIdAsyncCore(id, userId);
    }

    private async Task<TodoListDto?> GetTodoListByIdAsyncCore(int id, string userId)
    {
        var ownersTasks = await this._todoListRepository.GetByIdAsync(id);
        if (ownersTasks is null || ownersTasks.OwnerId != userId)
        {
            return null;
        }
        return new TodoListDto
        {
            Id = ownersTasks.Id,
            Title = ownersTasks.Title!,
            Description = ownersTasks.Description,
            CreatedDate = ownersTasks.CreatedDate,
            TaskCount = ownersTasks.Tasks?.Count ?? 0,
            OwnerId = userId
        };
    }

    public Task<IEnumerable<TodoTaskDto>> GetTasksByIdAsync(int id, string userId)
    {
        if (id <= 0)
        {
            throw new ArgumentException("List ID must be greater than zero", nameof(id));
        }
        if (string.IsNullOrEmpty(userId))
        {
            throw new ArgumentException("User ID cannot be null or empty", nameof(userId));
        }
        return GetTasksByIdAsyncCore(id, userId);
    }

    private async Task<IEnumerable<TodoTaskDto>> GetTasksByIdAsyncCore(int id, string userId)
    {
        var todoList = await _todoListRepository.GetByIdAsync(id);
        if (todoList == null)
        {
            throw new KeyNotFoundException($"Todo list with ID {id} not found");
        }

        if (todoList.OwnerId != userId)
        {
            throw new UnauthorizedAccessException("User does not have access to this todo list");
        }

        var tasks = await _todoTaskRepository.GetByListIdAsync(id);
        var taskDtos = tasks.Select(task => new TodoTaskDto
        {
            Id = task.Id,
            Title = task.Title!,
            Description = task.Description,
            IsCompleted = task.IsCompleted,
            Priority = task.Priority,
            CreatedDate = task.CreatedDate,
            CompletedDate = task.CompletedDate,
            TodoListId = task.TodoListId
        });
        return taskDtos;
    }

    public Task<TodoListDto> CreateAsync(TodoListDto todoListDto, string userId)
    {
        ArgumentNullException.ThrowIfNull(todoListDto);
        ArgumentNullException.ThrowIfNull(userId);
        return CreateAsyncCore(todoListDto, userId);
    }

    private async Task<TodoListDto> CreateAsyncCore(TodoListDto todoListDto, string userId)
    {
        TodoListEntity entity = new()
        {
            Title = todoListDto.Title,
            Description = todoListDto.Description,
            OwnerId = userId,
            CreatedDate = DateTime.UtcNow
        };
        var createdEntity = await this._todoListRepository.CreateAsync(entity);
        return new TodoListDto
        {
            Id = createdEntity.Id,
            Title = createdEntity.Title!,
            Description = createdEntity.Description,
            CreatedDate = createdEntity.CreatedDate,
            TaskCount = createdEntity.Tasks?.Count ?? 0,
        };
    }

    public Task UpdateAsync(TodoListDto todoListDto, string userId)
    {
        ArgumentNullException.ThrowIfNull(todoListDto);
        ArgumentNullException.ThrowIfNull(userId);
        return UpdateAsyncCore(todoListDto, userId);
    }

    private async Task UpdateAsyncCore(TodoListDto todoListDto, string userId)
    {
        var isOwner = await this._todoListRepository.IsOwnerAsync(todoListDto.Id, userId);
        if (!isOwner)
        {
            throw new UnauthorizedAccessException("User is not the owner of the todo list.");
        }
        var entity = await this._todoListRepository.GetByIdAsync(todoListDto.Id);
        entity!.Title = todoListDto.Title;
        entity.Description = todoListDto.Description;
        await this._todoListRepository.UpdateSync(entity);
    }

    public Task DeleteAsync(int id, string userId)
    {
        if (id <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(id));
        }
        ArgumentNullException.ThrowIfNull(userId);
        return DeleteAsyncCore(id, userId);
    }

    private async Task DeleteAsyncCore(int id, string userId)
    {
        var isOwner = await this._todoListRepository.IsOwnerAsync(id, userId);
        if (!isOwner)
        {
            throw new UnauthorizedAccessException("User is not the owner of the todo list.");
        }
        await this._todoListRepository.DeleteAsync(id);
    }
}
