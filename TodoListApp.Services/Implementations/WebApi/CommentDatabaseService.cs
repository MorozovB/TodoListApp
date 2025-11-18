using Microsoft.EntityFrameworkCore;
using TodoListApp.DataAccess.Context;
using TodoListApp.Entities;
using TodoListApp.Models.DTOs;
using TodoListApp.Services.Interfaces;

namespace TodoListApp.Services.Implementations.WebApi;

public class CommentDatabaseService : ICommentService
{
    private readonly TodoListDbContext _context;

    public CommentDatabaseService(TodoListDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<CommentDto>> GetCommentsByTaskIdAsync(int taskId, string userId)
    {
        var task = await _context.Tasks
            .Include(t => t.TodoList)
            .Include(t => t.Comments)
            .FirstOrDefaultAsync(t => t.Id == taskId);

        if (task == null)
        {
            throw new KeyNotFoundException($"Task with ID {taskId} not found");
        }

        if (task.TodoList!.OwnerId != userId && task.AssignedToUserId != userId)
        {
            throw new UnauthorizedAccessException("You don't have permission to view this task");
        }

        return task.Comments.Select(c => new CommentDto
        {
            Id = c.Id,
            TaskId = c.TaskId,
            Content = c.Content,
            CreatedDate = c.CreatedDate,
            CreatedBy = c.CreatedBy
        }).OrderByDescending(c => c.CreatedDate);
    }

    public async Task<CommentDto> AddCommentAsync(int taskId, string content, string userId)
    {
        var task = await _context.Tasks
            .Include(t => t.TodoList)
            .FirstOrDefaultAsync(t => t.Id == taskId);

        if (task == null)
        {
            throw new KeyNotFoundException($"Task with ID {taskId} not found");
        }

        if (task.TodoList!.OwnerId != userId && task.AssignedToUserId != userId)
        {
            throw new UnauthorizedAccessException("You don't have permission to comment on this task");
        }

        var comment = new Comment
        {
            TaskId = taskId,
            Content = content,
            CreatedDate = DateTime.UtcNow,
            CreatedBy = userId
        };

        _context.Comments.Add(comment);
        await _context.SaveChangesAsync();

        return new CommentDto
        {
            Id = comment.Id,
            TaskId = comment.TaskId,
            Content = comment.Content,
            CreatedDate = comment.CreatedDate,
            CreatedBy = comment.CreatedBy
        };
    }

    public async Task<CommentDto> UpdateCommentAsync(int commentId, string content, string userId)
    {
        var comment = await _context.Comments
            .Include(c => c.Task)
            .ThenInclude(t => t!.TodoList)
            .FirstOrDefaultAsync(c => c.Id == commentId);

        if (comment == null)
        {
            throw new KeyNotFoundException($"Comment with ID {commentId} not found");
        }

        if (comment.Task!.TodoList!.OwnerId != userId)
        {
            throw new UnauthorizedAccessException("Only list owner can edit comments");
        }

        comment.Content = content;
        await _context.SaveChangesAsync();

        return new CommentDto
        {
            Id = comment.Id,
            TaskId = comment.TaskId,
            Content = comment.Content,
            CreatedDate = comment.CreatedDate,
            CreatedBy = comment.CreatedBy
        };
    }

    public async Task DeleteCommentAsync(int commentId, string userId)
    {
        var comment = await _context.Comments
            .Include(c => c.Task)
            .ThenInclude(t => t!.TodoList)
            .FirstOrDefaultAsync(c => c.Id == commentId);

        if (comment == null)
        {
            throw new KeyNotFoundException($"Comment with ID {commentId} not found");
        }

        if (comment.Task!.TodoList!.OwnerId != userId)
        {
            throw new UnauthorizedAccessException("Only list owner can delete comments");
        }

        _context.Comments.Remove(comment);
        await _context.SaveChangesAsync();
    }
}
