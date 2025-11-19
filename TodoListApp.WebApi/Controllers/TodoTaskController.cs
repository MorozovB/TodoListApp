using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using TodoListApp.Models.DTOs;
using TodoListApp.Services.Interfaces;

namespace TodoListApp.WebApi.Controllers;

[ApiController]
[Route("api/task")]
public class TodoTaskController : ControllerBase
{
    private readonly ITodoTaskService _todoTaskService;

    public TodoTaskController(ITodoTaskService todoTaskservice)
    {
        this._todoTaskService = todoTaskservice ?? throw new ArgumentNullException(nameof(todoTaskservice));
    }

    [HttpGet("{listId}/tasks")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetByListIdAsync(int listId, [FromHeader(Name = "X-User-Id")] string? userIdFromHeader = null)
    {
        try
        {
            var userId = this.User.FindFirstValue(ClaimTypes.NameIdentifier) ?? userIdFromHeader;

            if (listId <= 0)
            {
                return BadRequest(new { message = "List ID can't be zero or negative" });
            }

            if (string.IsNullOrEmpty(userId))
            {
                return this.Unauthorized(new { message = "User ID not found" });
            }

            var allTask = await _todoTaskService.GetTasksByListIdAsync(listId, userId);

            return Ok(allTask);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (UnauthorizedAccessException ex)
        {
            return StatusCode(StatusCodes.Status403Forbidden, new { message = ex.Message });
        }
    }

    [HttpGet("{taskId}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetByTaskIdAsync(int taskId, [FromHeader(Name = "X-User-Id")] string? userIdFromHeader = null)
    {
        try
        {
            if (taskId <= 0)
            {
                return BadRequest(new { message = "Task ID must be greater than zero." });
            }

            var userId = this.User.FindFirstValue(ClaimTypes.NameIdentifier) ?? userIdFromHeader;

            if (string.IsNullOrEmpty(userId))
            {
                return this.Unauthorized(new { message = "User ID not found" });
            }

            var task = await _todoTaskService.GetByIdAsync(taskId, userId);

            if (task == null)
            {
                return NotFound(new { message = $"Task with ID {taskId} not found" });
            }

            return Ok(task);

        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (UnauthorizedAccessException ex)
        {
            return StatusCode(StatusCodes.Status403Forbidden, new { message = ex.Message });
        }
    }

    [HttpPost("{listId}/tasks")]
    [ProducesResponseType(typeof(TodoTaskDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> CreateTaskAsync(int listId, [FromBody] TodoTaskDto taskDto, [FromHeader(Name = "X-User-Id")] string? userIdFromHeader = null)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (listId <= 0)
            {
                return BadRequest(new { message = "List ID must be greater than zero" });
            }

            var userId = this.User.FindFirstValue(ClaimTypes.NameIdentifier) ?? userIdFromHeader;

            if (string.IsNullOrEmpty(userId))
            {
                return this.Unauthorized(new { message = "User ID not found" });
            }

            var createdTask = await _todoTaskService.CreateAsync(taskDto, listId, userId);

            return CreatedAtAction(nameof(GetByTaskIdAsync), new { id = createdTask.Id }, createdTask);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (UnauthorizedAccessException ex)
        {
            return StatusCode(StatusCodes.Status403Forbidden, new { message = ex.Message });
        }
    }

    [HttpPut("{taskId}")]
    [ProducesResponseType(typeof(TodoTaskDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> UpdateTaskAsync(int taskId, [FromBody] TodoTaskDto taskDto, [FromHeader(Name = "X-User-Id")] string? userIdFromHeader = null)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (taskId <= 0)
            {
                return BadRequest(new { message = "Task ID must be greater than zero" });
            }

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? userIdFromHeader;

            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized(new { message = "User ID not found" });
            }

            if (taskId != taskDto.Id)
            {
                return BadRequest(new { message = "Task ID in route doesn't match Task ID in body" });
            }

            var taskUpdated = await _todoTaskService.UpdateAsync(taskDto, userId);

            return Ok(taskUpdated);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (UnauthorizedAccessException ex)
        {
            return StatusCode(StatusCodes.Status403Forbidden, new { message = ex.Message });
        }
    }

    [HttpPatch("{taskId}/toggle")]
    [ProducesResponseType(typeof(TodoTaskDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> ToggleStatusOfTaskAsync(int taskId, [FromHeader(Name = "X-User-Id")] string? userIdFromHeader = null)
    {
        try
        {
            if (taskId <= 0)
            {
                return BadRequest(new { message = "Task ID must be greater than zero" });
            }

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? userIdFromHeader;

            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized(new { message = "User ID not found" });
            }

            var toggledTask = await _todoTaskService.ToggleCompletedAsync(taskId, userId);

            return Ok(toggledTask);

        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (UnauthorizedAccessException ex)
        {
            return StatusCode(StatusCodes.Status403Forbidden, new { message = ex.Message });
        }
    }

    [HttpDelete("{taskId}")]
    [ProducesResponseType(typeof(TodoTaskDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> DeleteTaskAsync(int taskId, [FromHeader(Name = "X-User-Id")] string? userIdFromHeader = null)
    {
        try
        {
            if (taskId <= 0)
            {
                return BadRequest(new { message = "Task ID must be greater than zero" });
            }

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? userIdFromHeader;

            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized(new { message = "User ID not found" });
            }

            await _todoTaskService.DeleteAsync(taskId, userId);

            return NoContent();
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (UnauthorizedAccessException ex)
        {
            return StatusCode(StatusCodes.Status403Forbidden, new { message = ex.Message });
        }
    }

}
