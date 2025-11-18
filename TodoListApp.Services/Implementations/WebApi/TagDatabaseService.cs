using Microsoft.EntityFrameworkCore;
using TodoListApp.DataAccess.Context;
using TodoListApp.Entities;
using TodoListApp.Models.DTOs;
using TodoListApp.Services.Interfaces;

namespace TodoListApp.Services.Implementations.WebApi;

public class TagDatabaseService : ITagService
{
    private readonly TodoListDbContext _context;

    public TagDatabaseService(TodoListDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<TagDto>> GetAllTagsAsync(string userId)
    {
        var tags = await _context.Set<Tag>()
            .Include(t => t.Tasks)
            .ThenInclude(task => task.TodoList)
            .Where(t => t.Tasks.Any(task => task.TodoList!.OwnerId == userId || task.AssignedToUserId == userId))
            .Select(t => new TagDto
            {
                Id = t.Id,
                Name = t.Name!,
                TaskCount = t.Tasks.Count(task => task.TodoList!.OwnerId == userId || task.AssignedToUserId == userId)
            })
            .OrderBy(t => t.Name)
            .ToListAsync();

        return tags;
    }

    public async Task<IEnumerable<TagDto>> GetTagsByTaskIdAsync(int taskId, string userId)
    {
        var task = await _context.Tasks
            .Include(t => t.TodoList)
            .Include(t => t.Tags)
            .FirstOrDefaultAsync(t => t.Id == taskId);

        if (task == null)
        {
            throw new KeyNotFoundException($"Task with ID {taskId} not found");
        }

        if (task.TodoList!.OwnerId != userId && task.AssignedToUserId != userId)
        {
            throw new UnauthorizedAccessException("You don't have permission to view this task");
        }

        return task.Tags.Select(tag => new TagDto
        {
            Id = tag.Id,
            Name = tag.Name!,
            TaskCount = 0
        });
    }

    public async Task<IEnumerable<TodoTaskDto>> GetTasksByTagIdAsync(int tagId, string userId)
    {
        var tasks = await _context.Tasks
            .Include(t => t.TodoList)
            .Include(t => t.Tags)
            .Where(t => t.Tags.Any(tag => tag.Id == tagId) &&
                       (t.TodoList!.OwnerId == userId || t.AssignedToUserId == userId))
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
                TodoListId = t.TodoListId
            })
            .ToListAsync();

        return tasks;
    }

    public async Task<TagDto> AddTagToTaskAsync(int taskId, string tagName, string userId)
    {
        var task = await _context.Tasks
            .Include(t => t.TodoList)
            .Include(t => t.Tags)
            .FirstOrDefaultAsync(t => t.Id == taskId);

        if (task == null)
        {
            throw new KeyNotFoundException($"Task with ID {taskId} not found");
        }

        if (task.TodoList!.OwnerId != userId)
        {
            throw new UnauthorizedAccessException("Only list owner can add tags");
        }

        var tag = await GetOrCreateTagAsync(tagName);

        if (!task.Tags.Any(t => t.Id == tag.Id))
        {
            task.Tags.Add(new Tag { Id = tag.Id, Name = tag.Name });
            await _context.SaveChangesAsync();
        }

        return tag;
    }

    public async Task RemoveTagFromTaskAsync(int taskId, int tagId, string userId)
    {
        var task = await _context.Tasks
            .Include(t => t.TodoList)
            .Include(t => t.Tags)
            .FirstOrDefaultAsync(t => t.Id == taskId);

        if (task == null)
        {
            throw new KeyNotFoundException($"Task with ID {taskId} not found");
        }

        if (task.TodoList!.OwnerId != userId)
        {
            throw new UnauthorizedAccessException("Only list owner can remove tags");
        }

        var tag = task.Tags.FirstOrDefault(t => t.Id == tagId);
        if (tag != null)
        {
            task.Tags.Remove(tag);
            await _context.SaveChangesAsync();
        }
    }

    public async Task<TagDto> GetOrCreateTagAsync(string tagName)
    {
        tagName = tagName.Trim().ToLower();

        var existingTag = await _context.Set<Tag>()
            .FirstOrDefaultAsync(t => t.Name!.ToLower() == tagName);

        if (existingTag != null)
        {
            return new TagDto
            {
                Id = existingTag.Id,
                Name = existingTag.Name!,
                TaskCount = 0
            };
        }

        var newTag = new Tag { Name = tagName };
        _context.Set<Tag>().Add(newTag);
        await _context.SaveChangesAsync();

        return new TagDto
        {
            Id = newTag.Id,
            Name = newTag.Name!,
            TaskCount = 0
        };
    }
}
