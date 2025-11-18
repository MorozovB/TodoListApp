using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using TodoListApp.Services.Interfaces;

namespace TodoListApp.WebApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AssignedTasksController : ControllerBase
{
    private readonly IAssignedTasksService _assignedTasksService;
    private readonly ILogger<AssignedTasksController> _logger;

    public AssignedTasksController(IAssignedTasksService assignedTasksService, ILogger<AssignedTasksController> logger)
    {
        _assignedTasksService = assignedTasksService;
        _logger = logger;
    }

    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetAssignedTasks(
        [FromQuery] Entities.Enums.StatusOfTask? status = null,
        [FromQuery] string sortBy = "duedate",
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

            _logger.LogInformation("Fetching assigned tasks for user {UserId}", userId);

            var result = await _assignedTasksService.GetAssignedTasksAsync(userId, status, sortBy, pageNumber, pageSize);

            return Ok(result);
        }
        catch (UnauthorizedAccessException ex)
        {
            return StatusCode(StatusCodes.Status403Forbidden, new { message = ex.Message });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }

    [HttpPatch("{taskId}/status")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ChangeStatusOfTask(
        int taskId,
        [FromBody] Entities.Enums.StatusOfTask newStatus,
        [FromHeader(Name = "X-User-Id")] string? userIdFromHeader = null)
    {
        try
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? userIdFromHeader;

            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized(new { message = "User ID not found" });
            }

            _logger.LogInformation("Changing status of task {TaskId} to {Status} for user {UserId}",
                taskId, newStatus, userId);

            var result = await _assignedTasksService.ChangeStatusOfTaskAsync(taskId, newStatus, userId);

            return Ok(result);
        }
        catch (KeyNotFoundException ex)
        {
            _logger.LogWarning(ex, "Task not found");
            return NotFound(new { message = ex.Message });
        }
        catch (UnauthorizedAccessException ex)
        {
            _logger.LogWarning(ex, "Unauthorized access");
            return Forbid(ex.Message);
        }
    }

    [HttpPatch("{taskId}/assign")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> AssignTask(
        int taskId,
        [FromBody] string newUserId,
        [FromHeader(Name = "X-User-Id")] string? userIdFromHeader = null)
    {
        try
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? userIdFromHeader;

            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized(new { message = "User ID not found" });
            }

            _logger.LogInformation("Assigning task {TaskId} to user {NewUserId}", taskId, newUserId);

            var result = await _assignedTasksService.AssignTaskToUserAsync(taskId, newUserId, userId);

            return Ok(result);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (UnauthorizedAccessException ex)
        {
            return Forbid(ex.Message);
        }
    }
}
