using System.Net.Http.Headers;
using System.Text.Json;
using Microsoft.Extensions.Configuration;
using TodoListApp.Models.Common;
using TodoListApp.Models.DTOs;
using TodoListApp.Services.Exceptions;
using TodoListApp.Services.Interfaces;

namespace TodoListApp.Services.Implementations.WebApp;

public class SearchWebApiService : ISearchService
{
    private readonly HttpClient _httpClient;

    public SearchWebApiService(HttpClient httpClient, IConfiguration configuration)
    {
        _httpClient = httpClient;

        var _baseUrl = configuration["WebApi:BaseUrl"] ?? throw new InvalidOperationException("BaseUrl is not configured");
        var _bearerToken = configuration["WebApi:BearerToken"] ?? throw new InvalidOperationException("BearerToken is not configured");

        _httpClient.BaseAddress = new Uri(_baseUrl);
        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _bearerToken);
    }

    public async Task<PagedResult<TodoTaskDto>> SearchTasksAsync(
        string userId,
        string searchQuery,
        DateTime? startDate = null,
        DateTime? endDate = null,
        int pageNumber = 1,
        int pageSize = 10)
    {
        try
        {
            var queryParams = new List<string>
            {
                $"pageNumber={pageNumber}",
                $"pageSize={pageSize}"
            };

            if (!string.IsNullOrWhiteSpace(searchQuery))
            {
                queryParams.Add($"q={Uri.EscapeDataString(searchQuery)}");
            }

            if (startDate.HasValue)
            {
                queryParams.Add($"startDate={startDate.Value:yyyy-MM-dd}");
            }

            if (endDate.HasValue)
            {
                queryParams.Add($"endDate={endDate.Value:yyyy-MM-dd}");
            }

            var url = $"api/search/tasks?{string.Join("&", queryParams)}";

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
}
