using TodoListApp.Models.DTOs;

namespace TodoListApp.Services.Interfaces;

public interface ITagService
{
    Task<IEnumerable<TagDto>> GetAllTagsAsync(string userId);
    Task<IEnumerable<TagDto>> GetTagsByTaskIdAsync(int taskId, string userId);
    Task<IEnumerable<TodoTaskDto>> GetTasksByTagIdAsync(int tagId, string userId);
    Task<TagDto> AddTagToTaskAsync(int taskId, string tagName, string userId);
    Task RemoveTagFromTaskAsync(int taskId, int tagId, string userId);
    Task<TagDto> GetOrCreateTagAsync(string tagName);
}
