using System.Text;
using System.Text.Json;
using TodoListApp.Models.DTOs;
using TodoListApp.Services.Interfaces;

namespace TodoListApp.Services.Implementations.WebApp;
public class TodoTaskWebApiService : ITodoTaskService
{
    private readonly HttpClient _httpClient;
    public TodoTaskWebApiService(HttpClient httpClient)
    {
        this._httpClient = httpClient;
    }

    public async Task<IEnumerable<TodoTaskDto>> GetTasksByListIdAsync(int listId, string userId)
    {
        try
        {
            var url = $"api/task/{listId}/tasks";

            var request = new HttpRequestMessage(HttpMethod.Get, url);
            request.Headers.Add("X-User-Id", userId);

            var response = await this._httpClient.SendAsync(request);

            if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                throw new KeyNotFoundException($"List with ID {listId} not found");
            }

            if (response.StatusCode == System.Net.HttpStatusCode.Forbidden)
            {
                throw new UnauthorizedAccessException("Access to this list is forbidden");
            }

            if (response.StatusCode == System.Net.HttpStatusCode.BadRequest)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                throw new ArgumentException($"Bad request: {errorContent}");
            }

            response.EnsureSuccessStatusCode();

            var jsonString = await response.Content.ReadAsStringAsync();

            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
            };

            var tasks = JsonSerializer.Deserialize<List<TodoTaskDto>>(jsonString, options);

            return tasks ?? new List<TodoTaskDto>();
        }
        catch (JsonException ex)
        {
            throw new Exception("Error deserializing the response", ex);
        }
    }

    public async Task<TodoTaskDto> GetByIdAsync(int taskId, string userId)
    {
        try
        {
            var url = $"api/task/{taskId}";

            var request = new HttpRequestMessage(HttpMethod.Get, url);
            request.Headers.Add("X-User-Id", userId);

            var response = await this._httpClient.SendAsync(request);

            if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                throw new KeyNotFoundException($"Task with ID {taskId} not found");
            }

            if (response.StatusCode == System.Net.HttpStatusCode.Forbidden)
            {
                throw new UnauthorizedAccessException("Access to this task is forbidden");
            }

            if (response.StatusCode == System.Net.HttpStatusCode.BadRequest)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                throw new ArgumentException($"Bad request: {errorContent}");
            }

            response.EnsureSuccessStatusCode();

            var jsonString = await response.Content.ReadAsStringAsync();
            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
            };

            var taskDto = JsonSerializer.Deserialize<TodoTaskDto>(jsonString, options);

            return taskDto ?? new TodoTaskDto();
        }
        catch (JsonException ex)
        {
            throw new Exception("Error deserializing the response", ex);
        }
    }

    public async Task<TodoTaskDto> CreateAsync(TodoTaskDto dto, int listId, string userId)
    {
        try
        {
            var url = $"api/todolist/{listId}/tasks";
            var jsonContent = JsonSerializer.Serialize(dto);
            var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

            var request = new HttpRequestMessage(HttpMethod.Post, url)
            {
                Content = content
            };
            request.Headers.Add("X-User-Id", userId);

            var response = await this._httpClient.SendAsync(request);
            response.EnsureSuccessStatusCode();

            var jsonString = await response.Content.ReadAsStringAsync();
            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
            };

            var taskDto = JsonSerializer.Deserialize<TodoTaskDto>(jsonString, options);
            return taskDto ?? new TodoTaskDto();

        }
        catch (JsonException ex)
        {
            throw new Exception("Error deserializing the response", ex);
        }
    }

    public async Task<TodoTaskDto> UpdateAsync(TodoTaskDto dto, string userId)
    {
        try
        {
            var url = $"api/task/{dto.Id}";
            var jsonContent = JsonSerializer.Serialize(dto);
            var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

            var request = new HttpRequestMessage(HttpMethod.Put, url)
            {
                Content = content
            };
            request.Headers.Add("X-User-Id", userId);

            var response = await this._httpClient.SendAsync(request);
            response.EnsureSuccessStatusCode();

            var jsonString = await response.Content.ReadAsStringAsync();
            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
            };

            var taskDto = JsonSerializer.Deserialize<TodoTaskDto>(jsonString, options);

            return taskDto ?? new TodoTaskDto();

        }
        catch (JsonException ex)
        {
            throw new Exception("Error deserializing the response", ex);
        }
    }

    public async Task DeleteAsync(int taskId, string userId)
    {
        try
        {
            var url = $"api/task/{taskId}";

            var request = new HttpRequestMessage(HttpMethod.Delete, url);
            request.Headers.Add("X-User-Id", userId);

            var response = await this._httpClient.SendAsync(request);
            if (response.StatusCode == System.Net.HttpStatusCode.NotFound ||
                response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
            {
                throw new KeyNotFoundException($"TodoList with ID {taskId} not found.");
            }
            _ = response.EnsureSuccessStatusCode();
        }
        catch (Exception ex)
        {
            throw new Exception("Error deserializing the response", ex);
        }
    }

    public async Task<TodoTaskDto> ToggleCompletedAsync(int taskId, string userId)
    {
        try
        {
            var url = $"api/task/{taskId}/toggle";
            var request = new HttpRequestMessage(HttpMethod.Patch, url);
            request.Headers.Add("X-User-Id", userId);

            var response = await _httpClient.SendAsync(request);
            response.EnsureSuccessStatusCode();

            var jsonString = await response.Content.ReadAsStringAsync();
            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };

            var updatedTask = JsonSerializer.Deserialize<TodoTaskDto>(jsonString, options);
            return updatedTask ?? new TodoTaskDto();
        }
        catch (JsonException ex)
        {
            throw new Exception("Error deserializing the response", ex);
        }
    }

}
