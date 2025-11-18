using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using TodoListApp.Services.Interfaces;

namespace TodoListApp.WebApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class SearchController : ControllerBase
{
    private readonly ISearchService _searchService;
    private readonly ILogger<SearchController> _logger;

    public SearchController(ISearchService searchService, ILogger<SearchController> logger)
    {
        _searchService = searchService;
        _logger = logger;
    }

    [HttpGet("tasks")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> SearchTasks(
        [FromQuery] string? q = null,
        [FromQuery] DateTime? startDate = null,
        [FromQuery] DateTime? endDate = null,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10,
        [FromHeader(Name = "X-User-Id")] string? userIdFromHeader = null)
    {
        try
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? userIdFromHeader;

            if (string.IsNullOrEmpty(userId))
            {
                _logger.LogWarning("User ID not found in claims or headers");
                return Unauthorized(new { message = "User ID not found" });
            }

            if (pageNumber < 1 || pageSize < 1 || pageSize > 100)
            {
                return BadRequest(new { message = "Invalid pagination parameters" });
            }

            _logger.LogInformation("Searching tasks for user {UserId} with query {Query}", userId, q);

            var result = await _searchService.SearchTasksAsync(userId, q ?? "", startDate, endDate, pageNumber, pageSize);

            return Ok(result);
        }
        catch (ArgumentException ex)
        {
            this._logger.LogWarning(ex, "Error searching tasks");
            return StatusCode(500, new { message = "Internal server error" });
        }
    }
}
