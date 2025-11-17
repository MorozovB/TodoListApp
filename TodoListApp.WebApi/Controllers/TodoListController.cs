using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using TodoListApp.Models.DTOs;
using TodoListApp.Services.Interfaces;

namespace TodoListApp.WebApi.Controllers;

[ApiController]
[Route("api/[controller]")]
//[Authorize]
public class TodoListController : ControllerBase
{
    private readonly ITodoListService _todoListService;
    private readonly ILogger<TodoListController> _logger;
    public TodoListController(ITodoListService todoListService, ILogger<TodoListController> logger)
    {
        this._todoListService = todoListService ?? throw new ArgumentNullException(nameof(todoListService));
        this._logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }


    /// <summary>
    /// Gets paginated todo lists for the authenticated user.
    /// </summary>
    /// <param name="pageNumber">Number of page</param>
    /// <param name="pageSize">Size of page</param>
    /// <returns></returns>
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetUSerTodoLists(int pageNumber = 1, int pageSize = 20)
    {
        try
        {
            // Validate pagination parameters
            if (pageNumber < 1)
            {
                this._logger.LogWarning("Invalid pageNumber: {PageNumber}", pageNumber);
                return this.BadRequest(new { message = "Page number must be greater than 0" });
            }

            if (pageSize < 1 || pageSize > 100)
            {
                this._logger.LogWarning("Invalid pageSize: {PageSize}", pageSize);
                return this.BadRequest(new { message = "Page size must be between 1 and 100" });
            }

            // Extract user ID from claims or headers
            var userId = this.User.FindFirstValue(ClaimTypes.NameIdentifier); // ?? "test-user-id";

            if (string.IsNullOrEmpty(userId))
            {
                userId = Request.Headers["X-User-Id"].FirstOrDefault();
            }

            if (string.IsNullOrEmpty(userId))
            {
                this._logger.LogWarning("User ID not found in claims or headers");
                return this.Unauthorized(new { message = "User ID not found" });
            }

            this._logger.LogInformation("Fetching todo lists for user {UserId}, page {PageNumber}, size {PageSize}",
                    userId, pageNumber, pageSize);

            var todoLists = await this._todoListService.GetUserTodoListsAsync(userId, pageNumber, pageSize);

            this._logger.LogInformation(
                    "Successfully retrieved {Count} todo lists for user {UserId}",
                    todoLists.Items.Count, userId);

            return this.Ok(todoLists);
        }
        catch (ArgumentException ex)
        {
            // Error in input parameters
            this._logger.LogWarning(ex, "Validation error while fetching todo lists");
            return this.BadRequest(new { message = ex.Message });
        }
        catch (UnauthorizedAccessException ex)
        {
            // Permission issues
            this._logger.LogWarning(ex, "Unauthorized access attempt");
            return this.Forbid();
        }
    }


    /// <summary>
    /// Gets a specific todo list by its ID for the authenticated user.
    /// </summary>
    /// <param name="id">Id of user</param>
    /// <returns></returns>
    [HttpGet("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetTodoListById(int id)
    {
        try
        {
            // Validate ID parameter
            if (id <= 0)
            {
                this._logger.LogWarning("Invalid id: {Id}", id);
                return this.BadRequest(new { message = "Invalid todo list ID" });
            }

            // Extract user ID from claims or headers
            var userId = this.User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(userId))
            {
                userId = Request.Headers["X-User-Id"].FirstOrDefault();
            }
            if (string.IsNullOrEmpty(userId))
            {
                this._logger.LogWarning("User ID not found in claims or headers");
                return this.Unauthorized(new { message = "User ID not found" });
            }

            this._logger.LogInformation("Fetching todo list {Id} for user {UserId}", id, userId);

            // Call the service to get the todo list
            var todoList = await this._todoListService.GetTodoListByIdAsync(id, userId);

            if (todoList == null)
            {
                this._logger.LogWarning("Todo list {TodoListId} not found or user {UserId} has no access", id, userId);
                return this.NotFound(new { message = $"Todo list with id {id} not found" });
            }

            this._logger.LogInformation(
                    "Successfully retrieved todo list {TodoListId} for user {UserId}",
                    id, userId);

            return this.Ok(todoList);
        }
        catch (UnauthorizedAccessException ex)
        {
            // Permission issues
            this._logger.LogWarning(ex,
                "User attempted to access todo list {TodoListId} without permission", id);
            return this.Forbid();
        }
    }


    /// <summary>
    /// Creates a new todo list for the authenticated user.
    /// </summary>
    /// <returns></returns>
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> CreateTodoList(TodoListDto todoListDto)
    {
        try
        {
            if (!this.ModelState.IsValid)
            {
                this._logger.LogWarning("Invalid model state for create todo list request");
                return this.BadRequest(this.ModelState);
            }

            var userId = this.User.FindFirstValue(ClaimTypes.NameIdentifier); // ?? "test-user-id";

            if (string.IsNullOrEmpty(userId))
            {
                userId = Request.Headers["X-User-Id"].FirstOrDefault();
            }

            if (string.IsNullOrEmpty(userId))
            {
                this._logger.LogWarning("User ID not found in claims or headers");
                return this.Unauthorized(new { message = "User ID not found" });
            }

            this._logger.LogInformation("Creating todo list '{Title}' for user {UserId}",
                    todoListDto.Title, userId);

            // Call the service to create the todo list
            var createdTodoList = await this._todoListService.CreateAsync(todoListDto, userId);

            this._logger.LogInformation(
                    "Successfully created todo list {TodoListId} for user {UserId}",
                    createdTodoList.Id, userId);

            // Return the created todo list with 201 status
            return this.CreatedAtAction(nameof(GetTodoListById), new { id = createdTodoList.Id }, createdTodoList);
        }
        catch (ArgumentException ex)
        {
            // Error in input parameters
            this._logger.LogWarning(ex, "Validation error while creating todo list");
            return this.BadRequest(new { message = ex.Message });
        }
        catch (UnauthorizedAccessException ex)
        {
            // Permission issues
            this._logger.LogWarning(ex, "Unauthorized access attempt");
            return this.Forbid();
        }
    }

    /// <summary>
    /// Updates an existing todo list for the authenticated user.
    /// </summary>
    /// <param name="id">Id of user</param>
    /// <param name="todoListDto">Data transfer from Entity</param>
    /// <returns></returns>
    [HttpPut("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> UpdateTodoList(int id, TodoListDto todoListDto)
    {
        try
        {
            // Check for ID mismatch
            if (id != todoListDto.Id)
            {
                _logger.LogWarning(
                    "ID mismatch: route id {RouteId} != body id {BodyId}",
                    id, todoListDto.Id);
                return BadRequest(new { message = "ID mismatch" });
            }

            // Validate model state
            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Invalid model state for update todo list request");
                return BadRequest(ModelState);
            }

            // Get user ID from claims or headers
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(userId))
            {
                userId = Request.Headers["X-User-Id"].FirstOrDefault();
            }

            if (string.IsNullOrEmpty(userId))
            {
                _logger.LogWarning("User ID not found in claims or headers");
                return Unauthorized(new { message = "User ID not found" });
            }

            _logger.LogInformation(
                "Updating todo list {TodoListId} for user {UserId}",
                id, userId);

            // Call the service to update the todo list
            await _todoListService.UpdateAsync(todoListDto, userId);

            _logger.LogInformation(
                "Successfully updated todo list {TodoListId} for user {UserId}",
                id, userId);

            return NoContent();
        }
        catch (UnauthorizedAccessException ex)
        {
            _logger.LogWarning(ex,
                "User {UserId} attempted to update todo list {TodoListId} without permission",
                User.FindFirstValue(ClaimTypes.NameIdentifier), id);
            return Forbid();
        }
        catch (KeyNotFoundException ex)
        {
            _logger.LogWarning(ex, "Todo list {TodoListId} not found", id);
            return NotFound(new { message = $"Todo list with id {id} not found" });
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Validation error while updating todo list {TodoListId}", id);
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Deletes a todo list by its ID for the authenticated user.
    /// </summary>
    /// <param name="id">Delete of user</param>
    /// <returns></returns>
    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> DeleteTodoList(int id)
    {
        try
        {
            // Validate ID parameter
            if (id <= 0)
            {
                _logger.LogWarning("Invalid id: {Id}", id);
                return BadRequest(new { message = "Invalid todo list ID" });
            }
            // Get user ID from claims or headers
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                userId = Request.Headers["X-User-Id"].FirstOrDefault();
            }
            if (string.IsNullOrEmpty(userId))
            {
                _logger.LogWarning("User ID not found in claims or headers");
                return Unauthorized(new { message = "User ID not found" });
            }
            _logger.LogInformation("Deleting todo list {TodoListId} for user {UserId}", id, userId);
            // Call the service to delete the todo list
            await _todoListService.DeleteAsync(id, userId);
            _logger.LogInformation(
                "Successfully deleted todo list {TodoListId} for user {UserId}",
                id, userId);
            return NoContent();
        }
        catch (UnauthorizedAccessException ex)
        {
            _logger.LogWarning(ex,
                "User {UserId} attempted to delete todo list {TodoListId} without permission",
                User.FindFirstValue(ClaimTypes.NameIdentifier), id);
            return Forbid();
        }
        catch (KeyNotFoundException ex)
        {
            _logger.LogWarning(ex, "Todo list {TodoListId} not found", id);
            return NotFound(new { message = $"Todo list with id {id} not found" });
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Validation error while deleting todo list {TodoListId}", id);
            return BadRequest(new { message = ex.Message });
        }
    }
}
