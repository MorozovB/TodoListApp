using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Configuration;
using TodoListApp.Models.DTOs;
using TodoListApp.Services.Exceptions;
using TodoListApp.Services.Interfaces;

namespace TodoListApp.Services.Implementations.WebApp;

public class CommentWebApiService : ICommentService
{
    private readonly HttpClient _httpClient;

    public CommentWebApiService(HttpClient httpClient, IConfiguration configuration)
    {
        _httpClient = httpClient;

        var _baseUrl = configuration["WebApi:BaseUrl"] ?? throw new InvalidOperationException("BaseUrl is not configured");
        var _bearerToken = configuration["WebApi:BearerToken"] ?? throw new InvalidOperationException("BearerToken is not configured");

        _httpClient.BaseAddress = new Uri(_baseUrl);
        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _bearerToken);
    }

    public async Task<IEnumerable<CommentDto>> GetCommentsByTaskIdAsync(int taskId, string userId)
    {
        try
        {
            var request = new HttpRequestMessage(HttpMethod.Get, $"api/comments/task/{taskId}");
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

            var comments = JsonSerializer.Deserialize<List<CommentDto>>(jsonString, options);
            return comments ?? new List<CommentDto>();
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

    public async Task<CommentDto> AddCommentAsync(int taskId, string content, string userId)
    {
        try
        {
            var requestContent = new StringContent(
                JsonSerializer.Serialize(content),
                Encoding.UTF8,
                "application/json");

            var request = new HttpRequestMessage(HttpMethod.Post, $"api/comments/task/{taskId}")
            {
                Content = requestContent
            };
            request.Headers.Add("X-User-Id", userId);

            var response = await _httpClient.SendAsync(request);

            if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                throw new KeyNotFoundException($"Task with ID {taskId} not found");
            }

            if (response.StatusCode == System.Net.HttpStatusCode.Forbidden)
            {
                throw new UnauthorizedAccessException("You don't have permission to comment on this task");
            }

            response.EnsureSuccessStatusCode();

            var jsonString = await response.Content.ReadAsStringAsync();
            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };

            var comment = JsonSerializer.Deserialize<CommentDto>(jsonString, options);
            return comment ?? throw new ApiResponseException("Invalid response from server");
        }
        catch (HttpRequestException ex)
        {
            throw new ApiConnectionException("Unable to connect to the API server.", ex);
        }
    }

    public async Task<CommentDto> UpdateCommentAsync(int commentId, string content, string userId)
    {
        try
        {
            var requestContent = new StringContent(
                JsonSerializer.Serialize(content),
                Encoding.UTF8,
                "application/json");

            var request = new HttpRequestMessage(HttpMethod.Put, $"api/comments/{commentId}")
            {
                Content = requestContent
            };
            request.Headers.Add("X-User-Id", userId);

            var response = await _httpClient.SendAsync(request);

            if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                throw new KeyNotFoundException($"Comment with ID {commentId} not found");
            }

            if (response.StatusCode == System.Net.HttpStatusCode.Forbidden)
            {
                throw new UnauthorizedAccessException("Only list owner can edit comments");
            }

            response.EnsureSuccessStatusCode();

            var jsonString = await response.Content.ReadAsStringAsync();
            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };

            var comment = JsonSerializer.Deserialize<CommentDto>(jsonString, options);
            return comment ?? throw new ApiResponseException("Invalid response from server");
        }
        catch (HttpRequestException ex)
        {
            throw new ApiConnectionException("Unable to connect to the API server.", ex);
        }
    }

    public async Task DeleteCommentAsync(int commentId, string userId)
    {
        try
        {
            var request = new HttpRequestMessage(HttpMethod.Delete, $"api/comments/{commentId}");
            request.Headers.Add("X-User-Id", userId);

            var response = await _httpClient.SendAsync(request);

            if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                throw new KeyNotFoundException($"Comment with ID {commentId} not found");
            }

            if (response.StatusCode == System.Net.HttpStatusCode.Forbidden)
            {
                throw new UnauthorizedAccessException("Only list owner can delete comments");
            }

            response.EnsureSuccessStatusCode();
        }
        catch (HttpRequestException ex)
        {
            throw new ApiConnectionException("Unable to connect to the API server.", ex);
        }
    }
}
