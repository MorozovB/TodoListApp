using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using TodoListApp.Services.Interfaces;

namespace TodoListApp.WebApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CommentsController : ControllerBase
{
    private readonly ICommentService _commentService;

    public CommentsController(ICommentService commentService)
    {
        _commentService = commentService;
    }

    [HttpGet("task/{taskId}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetTaskComments(int taskId, [FromHeader(Name = "X-User-Id")] string? userIdFromHeader = null)
    {
        try
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? userIdFromHeader;

            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized(new { message = "User ID not found" });
            }

            var comments = await _commentService.GetCommentsByTaskIdAsync(taskId, userId);
            return Ok(comments);
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

    [HttpPost("task/{taskId}")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> AddComment(int taskId, [FromBody] string content, [FromHeader(Name = "X-User-Id")] string? userIdFromHeader = null)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(content))
            {
                return BadRequest(new { message = "Comment content cannot be empty" });
            }

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? userIdFromHeader;

            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized(new { message = "User ID not found" });
            }

            var comment = await _commentService.AddCommentAsync(taskId, content, userId);
            return CreatedAtAction(nameof(GetTaskComments), new { taskId }, comment);
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

    [HttpPut("{commentId}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateComment(int commentId, [FromBody] string content, [FromHeader(Name = "X-User-Id")] string? userIdFromHeader = null)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(content))
            {
                return BadRequest(new { message = "Comment content cannot be empty" });
            }

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? userIdFromHeader;

            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized(new { message = "User ID not found" });
            }

            var comment = await _commentService.UpdateCommentAsync(commentId, content, userId);
            return Ok(comment);
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

    [HttpDelete("{commentId}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteComment(int commentId, [FromHeader(Name = "X-User-Id")] string? userIdFromHeader = null)
    {
        try
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? userIdFromHeader;

            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized(new { message = "User ID not found" });
            }

            await _commentService.DeleteCommentAsync(commentId, userId);
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
