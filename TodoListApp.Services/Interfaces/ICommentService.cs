using TodoListApp.Models.DTOs;

namespace TodoListApp.Services.Interfaces;

public interface ICommentService
{
    Task<IEnumerable<CommentDto>> GetCommentsByTaskIdAsync(int taskId, string userId);
    Task<CommentDto> AddCommentAsync(int taskId, string content, string userId);
    Task<CommentDto> UpdateCommentAsync(int commentId, string content, string userId);
    Task DeleteCommentAsync(int commentId, string userId);
}
