using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Configuration;
using TodoListApp.Models.Common;
using TodoListApp.Models.DTOs;
using TodoListApp.Services.Exceptions;
using TodoListApp.Services.Interfaces;

namespace TodoListApp.Services.Implementations.WebApp;
public class TodoListWebApiService : ITodoListService
{
    private readonly HttpClient _httpClient;

    public TodoListWebApiService(HttpClient httpClient, IConfiguration configuration)
    {
        this._httpClient = httpClient;

        var _baseUrl = configuration["WebApi:BaseUrl"] ?? throw new InvalidOperationException("BaseUrl is not configured");
        var _bearerToken = configuration["WebApi:BearerToken"] ?? throw new InvalidOperationException("BearerToken is not configured");

        this._httpClient.BaseAddress = new Uri(_baseUrl);
        this._httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _bearerToken);
    }


    public async Task<PagedResult<TodoListDto>> GetUserTodoListsAsync(string userId, int pageNumber, int pageSize)
    {
        try
        {
            var url = $"api/todolist?pageNumber={pageNumber}&pageSize={pageSize}";

            var request = new HttpRequestMessage(HttpMethod.Get, url);

            request.Headers.Add("X-User-Id", userId);

            var response = await this._httpClient.SendAsync(request);

            _ = response.EnsureSuccessStatusCode();

            var jsonString = await response.Content.ReadAsStringAsync();

            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
            };
            var pagedResult = JsonSerializer.Deserialize<PagedResult<TodoListDto>>(jsonString, options);

            return pagedResult ?? new PagedResult<TodoListDto>
            {
                Items = new List<TodoListDto>(),
                TotalCount = 0,
                PageNumber = pageNumber,
                PageSize = pageSize
            };

        }
        catch (JsonException ex)
        {
            throw new InvalidOperationException("Error deserializing the response", ex);
        }
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
        try
        {
            var url = $"api/todolist/{id}/tasks";
            var request = new HttpRequestMessage(HttpMethod.Get, url);
            request.Headers.Add("X-User-Id", userId);
            var response = await _httpClient.SendAsync(request);

            if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                throw new KeyNotFoundException($"Todo list with ID {id} not found");
            }
            if (response.StatusCode == System.Net.HttpStatusCode.Forbidden ||
                response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
            {
                throw new UnauthorizedAccessException("You don't have permission to access this todo list");
            }
            if (!response.IsSuccessStatusCode)
            {
                var statusCode = (int)response.StatusCode;
                throw new ApiResponseException(
                    $"Failed to retrieve tasks. API returned status code: {statusCode}",
                    statusCode);
            }

            var jsonString = await response.Content.ReadAsStringAsync();
            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };
            var tasks = JsonSerializer.Deserialize<List<TodoTaskDto>>(jsonString, options);
            return tasks ?? new List<TodoTaskDto>();
        }
        catch (HttpRequestException ex)
        {
            throw new ApiConnectionException(
                "Unable to connect to the API server. Please check your connection.", ex);
        }
        catch (JsonException ex)
        {
            throw new ApiResponseException(
                "Received invalid response from API server.", ex);
        }
    }

    public async Task<TodoListDto?> GetTodoListByIdAsync(int id, string userId)
    {
        try
        {
            var url = $"api/todolist/{id}";

            var request = new HttpRequestMessage(HttpMethod.Get, url);

            request.Headers.Add("X-User-Id", userId);

            var response = await this._httpClient.SendAsync(request);

            if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                return null;
            }

            _ = response.EnsureSuccessStatusCode();

            var jsonString = await response.Content.ReadAsStringAsync();
            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
            };

            var todoList = JsonSerializer.Deserialize<TodoListDto>(jsonString, options);

            return todoList;
        }
        catch (JsonException ex)
        {
            throw new InvalidOperationException("Error deserializing the response", ex);
        }
    }

    public async Task<TodoListDto> CreateAsync(TodoListDto todoListDto, string userId)
    {
        try
        {
            var url = "api/todolist";

            var jsonContent = JsonSerializer.Serialize(todoListDto);
            var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

            var request = new HttpRequestMessage(HttpMethod.Post, url)
            {
                Content = content
            };
            request.Headers.Add("X-User-Id", userId);

            var response = await this._httpClient.SendAsync(request);

            _ = response.EnsureSuccessStatusCode();

            var jsonString = await response.Content.ReadAsStringAsync();

            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
            };

            var createdTodoList = JsonSerializer.Deserialize<TodoListDto>(jsonString, options);
            return createdTodoList ?? throw new InvalidOperationException("Failed to deserialize the created TodoList");
        }
        catch (JsonException ex)
        {
            throw new InvalidOperationException("Error deserializing the response", ex);
        }
    }

    public async Task UpdateAsync(TodoListDto todoListDto, string userId)
    {
        try
        {
            var url = $"api/todolist/{todoListDto.Id}";
            var jsonContent = JsonSerializer.Serialize(todoListDto);
            var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");
            var request = new HttpRequestMessage(HttpMethod.Put, url)
            {
                Content = content
            };
            request.Headers.Add("X-User-Id", userId);
            var response = await this._httpClient.SendAsync(request);

            if (response.StatusCode == System.Net.HttpStatusCode.NotFound ||
                response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
            {
                throw new KeyNotFoundException($"TodoList with ID {todoListDto.Id} not found.");
            }

            _ = response.EnsureSuccessStatusCode();
        }
        catch (JsonException ex)
        {
            throw new InvalidOperationException("Error deserializing the response", ex);
        }
    }

    public async Task DeleteAsync(int id, string userId)
    {
        try
        {
            var url = $"/api/todolist/{id}";
            var request = new HttpRequestMessage(HttpMethod.Delete, url);
            request.Headers.Add("X-User-Id", userId);
            var response = await this._httpClient.SendAsync(request);
            if (response.StatusCode == System.Net.HttpStatusCode.NotFound ||
                response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
            {
                throw new KeyNotFoundException($"TodoList with ID {id} not found.");
            }
            _ = response.EnsureSuccessStatusCode();
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException("Error deserializing the response", ex);
        }
    }
}
