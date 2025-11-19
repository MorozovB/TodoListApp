using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Configuration;
using TodoListApp.Models.DTOs;
using TodoListApp.Services.Exceptions;
using TodoListApp.Services.Interfaces;

namespace TodoListApp.Services.Implementations.WebApp;

public class TagWebApiService : ITagService
{
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _configuration;
    private readonly string _baseUrl;
    private readonly string _bearerToken;

    public TagWebApiService(HttpClient httpClient, IConfiguration configuration)
    {
        _httpClient = httpClient;
        _configuration = configuration;
        _baseUrl = _configuration["WebApi:BaseUrl"] ?? throw new InvalidOperationException("BaseUrl is not configured");
        _bearerToken = _configuration["WebApi:BearerToken"] ?? throw new InvalidOperationException("BearerToken is not configured");

        _httpClient.BaseAddress = new Uri(_baseUrl);
        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _bearerToken);
    }

    public async Task<IEnumerable<TagDto>> GetAllTagsAsync(string userId)
    {
        try
        {
            var request = new HttpRequestMessage(HttpMethod.Get, "api/tags");
            request.Headers.Add("X-User-Id", userId);

            var response = await _httpClient.SendAsync(request);
            response.EnsureSuccessStatusCode();

            var jsonString = await response.Content.ReadAsStringAsync();
            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };

            var tags = JsonSerializer.Deserialize<List<TagDto>>(jsonString, options);
            return tags ?? new List<TagDto>();
        }
        catch (HttpRequestException ex)
        {
            throw new ApiConnectionException("Unable to connect to the API server.", ex);
        }
        catch (JsonException ex)
        {
            throw new ApiResponseException("Invalid response from API server.", ex);
        }
    }

    public async Task<IEnumerable<TagDto>> GetTagsByTaskIdAsync(int taskId, string userId)
    {
        try
        {
            var request = new HttpRequestMessage(HttpMethod.Get, $"api/tags/task/{taskId}");
            request.Headers.Add("X-User-Id", userId);

            var response = await _httpClient.SendAsync(request);

            if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                throw new KeyNotFoundException($"Task with ID {taskId} not found");
            }

            response.EnsureSuccessStatusCode();

            var jsonString = await response.Content.ReadAsStringAsync();
            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };

            var tags = JsonSerializer.Deserialize<List<TagDto>>(jsonString, options);
            return tags ?? new List<TagDto>();
        }
        catch (HttpRequestException ex)
        {
            throw new ApiConnectionException("Unable to connect to the API server.", ex);
        }
    }

    public async Task<IEnumerable<TodoTaskDto>> GetTasksByTagIdAsync(int tagId, string userId)
    {
        try
        {
            var request = new HttpRequestMessage(HttpMethod.Get, $"api/tags/{tagId}/tasks");
            request.Headers.Add("X-User-Id", userId);

            var response = await _httpClient.SendAsync(request);
            response.EnsureSuccessStatusCode();

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
            throw new ApiConnectionException("Unable to connect to the API server.", ex);
        }
    }

    public async Task<TagDto> AddTagToTaskAsync(int taskId, string tagName, string userId)
    {
        try
        {
            var content = new StringContent(
                JsonSerializer.Serialize(tagName),
                Encoding.UTF8,
                "application/json");

            var request = new HttpRequestMessage(HttpMethod.Post, $"api/tags/task/{taskId}")
            {
                Content = content
            };
            request.Headers.Add("X-User-Id", userId);

            var response = await _httpClient.SendAsync(request);

            if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                throw new KeyNotFoundException($"Task with ID {taskId} not found");
            }

            if (response.StatusCode == System.Net.HttpStatusCode.Forbidden)
            {
                throw new UnauthorizedAccessException("Only list owner can add tags");
            }

            response.EnsureSuccessStatusCode();

            var jsonString = await response.Content.ReadAsStringAsync();
            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };

            var tag = JsonSerializer.Deserialize<TagDto>(jsonString, options);
            return tag ?? throw new ApiResponseException("Invalid response from server");
        }
        catch (HttpRequestException ex)
        {
            throw new ApiConnectionException("Unable to connect to the API server.", ex);
        }
    }

    public async Task RemoveTagFromTaskAsync(int taskId, int tagId, string userId)
    {
        try
        {
            var request = new HttpRequestMessage(HttpMethod.Delete, $"api/tags/task/{taskId}/{tagId}");
            request.Headers.Add("X-User-Id", userId);

            var response = await _httpClient.SendAsync(request);

            if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                throw new KeyNotFoundException("Task or tag not found");
            }

            if (response.StatusCode == System.Net.HttpStatusCode.Forbidden)
            {
                throw new UnauthorizedAccessException("Only list owner can remove tags");
            }

            response.EnsureSuccessStatusCode();
        }
        catch (HttpRequestException ex)
        {
            throw new ApiConnectionException("Unable to connect to the API server.", ex);
        }
    }

    public async Task<TagDto> GetOrCreateTagAsync(string tagName)
    {
        return new TagDto
        {
            Id = 0,
            Name = tagName,
            TaskCount = 0
        };
    }
}
