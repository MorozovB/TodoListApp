using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using TodoListApp.Services.Interfaces;

namespace TodoListApp.WebApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TagsController : ControllerBase
{
    private readonly ITagService _tagService;

    public TagsController(ITagService tagService)
    {
        _tagService = tagService;
    }

    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetAllTags([FromHeader(Name = "X-User-Id")] string? userIdFromHeader = null)
    {
        try
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? userIdFromHeader;

            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized(new { message = "User ID not found" });
            }

            var tags = await _tagService.GetAllTagsAsync(userId);
            return Ok(tags);
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

    [HttpGet("task/{taskId}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetTaskTags(int taskId, [FromHeader(Name = "X-User-Id")] string? userIdFromHeader = null)
    {
        try
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? userIdFromHeader;

            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized(new { message = "User ID not found" });
            }

            var tags = await _tagService.GetTagsByTaskIdAsync(taskId, userId);
            return Ok(tags);
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

    [HttpGet("{tagId}/tasks")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetTasksByTag(int tagId, [FromHeader(Name = "X-User-Id")] string? userIdFromHeader = null)
    {
        try
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? userIdFromHeader;

            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized(new { message = "User ID not found" });
            }

            var tasks = await _tagService.GetTasksByTagIdAsync(tagId, userId);
            return Ok(tasks);
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

    [HttpPost("task/{taskId}")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> AddTagToTask(int taskId, [FromBody] string tagName, [FromHeader(Name = "X-User-Id")] string? userIdFromHeader = null)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(tagName))
            {
                return BadRequest(new { message = "Tag name cannot be empty" });
            }

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? userIdFromHeader;

            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized(new { message = "User ID not found" });
            }

            var tag = await _tagService.AddTagToTaskAsync(taskId, tagName, userId);
            return CreatedAtAction(nameof(GetTaskTags), new { taskId }, tag);
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

    [HttpDelete("task/{taskId}/{tagId}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> RemoveTagFromTask(int taskId, int tagId, [FromHeader(Name = "X-User-Id")] string? userIdFromHeader = null)
    {
        try
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? userIdFromHeader;

            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized(new { message = "User ID not found" });
            }

            await _tagService.RemoveTagFromTaskAsync(taskId, tagId, userId);
            return NoContent();
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
