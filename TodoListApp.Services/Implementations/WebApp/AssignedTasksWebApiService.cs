using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Configuration;
using TodoListApp.Models.Common;
using TodoListApp.Models.DTOs;
using TodoListApp.Services.Exceptions;
using TodoListApp.Services.Interfaces;

namespace TodoListApp.Services.Implementations.WebApp;

public class AssignedTasksWebApiService : IAssignedTasksService
{
    private readonly HttpClient _httpClient;

    public AssignedTasksWebApiService(HttpClient httpClient, IConfiguration configuration)
    {
        _httpClient = httpClient;

        var _baseUrl = configuration["WebApi:BaseUrl"] ?? throw new InvalidOperationException("BaseUrl is not configured");
        var _bearerToken = configuration["WebApi:BearerToken"] ?? throw new InvalidOperationException("BearerToken is not configured");

        _httpClient.BaseAddress = new Uri(_baseUrl);
        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _bearerToken);
    }

    public async Task<PagedResult<TodoTaskDto>> GetAssignedTasksAsync(
        string userId,
        Entities.Enums.StatusOfTask? statusFilter = null,
        string sortBy = "duedate",
        int pageNumber = 1,
        int pageSize = 10)
    {
        try
        {
            var url = $"api/assignedtasks?pageNumber={pageNumber}&pageSize={pageSize}&sortBy={sortBy}";

            if (statusFilter.HasValue)
            {
                url += $"&status={(int)statusFilter.Value}";
            }

            var request = new HttpRequestMessage(HttpMethod.Get, url);
            request.Headers.Add("X-User-Id", userId);

            var response = await _httpClient.SendAsync(request);
            response.EnsureSuccessStatusCode();

            var jsonString = await response.Content.ReadAsStringAsync();
            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };

            var pagedResult = JsonSerializer.Deserialize<PagedResult<TodoTaskDto>>(jsonString, options);

            return pagedResult ?? new PagedResult<TodoTaskDto>
            {
                Items = new List<TodoTaskDto>(),
                TotalCount = 0,
                PageNumber = pageNumber,
                PageSize = pageSize
            };
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

    public async Task<TodoTaskDto> ChangeStatusOfTaskAsync(int taskId, Entities.Enums.StatusOfTask newStatus, string userId)
    {
        try
        {
            var url = $"api/assignedtasks/{taskId}/status";

            var content = new StringContent(
                JsonSerializer.Serialize((int)newStatus),
                Encoding.UTF8,
                "application/json");

            var request = new HttpRequestMessage(HttpMethod.Patch, url)
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
                throw new UnauthorizedAccessException("You don't have permission to change this task");
            }

            response.EnsureSuccessStatusCode();

            var jsonString = await response.Content.ReadAsStringAsync();
            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };

            var updatedTask = JsonSerializer.Deserialize<TodoTaskDto>(jsonString, options);
            return updatedTask ?? throw new ApiResponseException("Invalid response from server");
        }
        catch (HttpRequestException ex)
        {
            throw new ApiConnectionException("Unable to connect to the API server.", ex);
        }
    }

    public async Task<TodoTaskDto> AssignTaskToUserAsync(int taskId, string newUserId, string currentUserId)
    {
        try
        {
            var url = $"api/assignedtasks/{taskId}/assign";

            var content = new StringContent(
                JsonSerializer.Serialize(newUserId),
                Encoding.UTF8,
                "application/json");

            var request = new HttpRequestMessage(HttpMethod.Patch, url)
            {
                Content = content
            };
            request.Headers.Add("X-User-Id", currentUserId);

            var response = await _httpClient.SendAsync(request);

            if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                throw new KeyNotFoundException($"Task with ID {taskId} not found");
            }

            if (response.StatusCode == System.Net.HttpStatusCode.Forbidden)
            {
                throw new UnauthorizedAccessException("Only list owner can reassign tasks");
            }

            response.EnsureSuccessStatusCode();

            var jsonString = await response.Content.ReadAsStringAsync();
            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };

            var updatedTask = JsonSerializer.Deserialize<TodoTaskDto>(jsonString, options);
            return updatedTask ?? throw new ApiResponseException("Invalid response from server");
        }
        catch (HttpRequestException ex)
        {
            throw new ApiConnectionException("Unable to connect to the API server.", ex);
        }
    }
}
