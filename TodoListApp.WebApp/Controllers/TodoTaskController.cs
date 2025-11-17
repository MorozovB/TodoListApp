using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TodoListApp.Models.ViewModels;
using TodoListApp.Services.Exceptions;
using TodoListApp.Services.Interfaces;
using TodoListApp.Models.DTOs;

namespace TodoListApp.WebApp.Controllers;

[Authorize]
public class TodoTaskController : Controller
{
    private readonly ITodoTaskService _todoTaskService;
    private readonly ITodoListService _todoListService;
    private readonly ILogger<TodoTaskController> _logger;

    public TodoTaskController(
        ITodoTaskService todoTaskService,
        ITodoListService todoListService,
        ILogger<TodoTaskController> logger)
    {
        _todoTaskService = todoTaskService;
        _todoListService = todoListService;
        _logger = logger;
    }

    [HttpGet]
    public async Task<IActionResult> Details(int id)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId))
        {
            return RedirectToAction("Login", "Account");
        }

        try
        {
            var taskDto = await _todoTaskService.GetByIdAsync(id, userId);

            if (taskDto == null)
            {
                TempData["ErrorMessage"] = "Task not found.";
                return RedirectToAction("Index", "TodoList");
            }

            // Get the todo list to display its title
            var todoListDto = await _todoListService.GetTodoListByIdAsync(taskDto.TodoListId, userId);

            if (todoListDto == null)
            {
                TempData["ErrorMessage"] = "Access denied or list not found.";
                return RedirectToAction("Index", "TodoList");
            }

            var viewModel = new TodoTaskDetailsViewModel
            {
                Id = taskDto.Id,
                Title = taskDto.Title,
                Description = taskDto.Description,
                IsCompleted = taskDto.IsCompleted,
                Priority = (int)taskDto.Priority,  // Явное преобразование enum → int
                CreatedDate = taskDto.CreatedDate,
                CompletedDate = taskDto.CompletedDate,
                DueDate = taskDto.DueDate,
                TodoListId = taskDto.TodoListId,
                TodoListTitle = todoListDto.Title,
                IsOverdue = taskDto.DueDate.HasValue &&
                           taskDto.DueDate.Value.Date < DateTime.UtcNow.Date &&
                           !taskDto.IsCompleted
            };

            return View(viewModel);
        }
        catch (ApiConnectionException)
        {
            TempData["ErrorMessage"] = "Unable to connect to the server. Please try again later.";
            return RedirectToAction("Index", "TodoList");
        }
        catch (UnauthorizedAccessException)
        {
            TempData["ErrorMessage"] = "You don't have permission to view this task.";
            return RedirectToAction("Index", "TodoList");
        }
    }


    [HttpGet]
    public async Task<IActionResult> Create(int todoListId)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId))
        {
            return RedirectToAction("Login", "Account");
        }

        try
        {
            var todoList = await _todoListService.GetTodoListByIdAsync(todoListId, userId);
            if (todoList == null)
            {
                TempData["ErrorMessage"] = "List not found or access denied.";
                return RedirectToAction("Index", "TodoList");
            }

            var viewModel = new TodoTaskCreateViewModel
            {
                TodoListId = todoListId,
                TodoListTitle = todoList.Title
            };

            return View(viewModel);
        }
        catch (ApiConnectionException)
        {
            TempData["ErrorMessage"] = "Unable to connect to the server.";
            return RedirectToAction("Index", "TodoList");
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(int todoListId, TodoTaskCreateViewModel model)
    {
        if (!ModelState.IsValid)
        {
            var todoList = await _todoListService.GetTodoListByIdAsync(todoListId,
                User.FindFirstValue(ClaimTypes.NameIdentifier));
            model.TodoListTitle = todoList?.Title ?? "Unknown";
            return View(model);
        }

        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId))
        {
            return RedirectToAction("Login", "Account");
        }

        try
        {
            var dto = new TodoTaskDto
            {
                Title = model.Title,
                Description = model.Description,
                DueDate = model.DueDate ?? DateTime.MinValue,
                Priority = (Entities.Enums.TaskPriority)model.Priority,
                TodoListId = todoListId
            };

            await _todoTaskService.CreateAsync(dto, todoListId, userId);

            TempData["SuccessMessage"] = "Task created successfully.";
            return RedirectToAction("Details", "TodoList", new { id = todoListId });
        }
        catch (ApiConnectionException)
        {
            ModelState.AddModelError("", "Unable to connect to the server. Please try again later.");
            return View(model);
        }
        catch (KeyNotFoundException ex)
        {
            ModelState.AddModelError("", ex.Message);
            return View(model);
        }
    }

    [HttpGet]
    public async Task<IActionResult> Edit(int id)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId))
        {
            return RedirectToAction("Login", "Account");
        }

        try
        {
            var taskDto = await _todoTaskService.GetByIdAsync(id, userId);

            if (taskDto == null)
            {
                TempData["ErrorMessage"] = "Task not found.";
                return RedirectToAction("Index", "TodoList");
            }

            var todoListDto = await _todoListService.GetTodoListByIdAsync(taskDto.TodoListId, userId);

            var viewModel = new TodoTaskEditViewModel
            {
                Id = taskDto.Id,
                Title = taskDto.Title,
                Description = taskDto.Description,
                DueDate = taskDto.DueDate,
                Priority = (int)taskDto.Priority,
                TodoListId = taskDto.TodoListId,
                TodoListTitle = todoListDto?.Title ?? "Unknown"
            };

            return View(viewModel);
        }
        catch (ApiConnectionException)
        {
            TempData["ErrorMessage"] = "Unable to connect to the server.";
            return RedirectToAction("Index", "TodoList");
        }
        catch (UnauthorizedAccessException)
        {
            TempData["ErrorMessage"] = "You don't have permission to edit this task.";
            return RedirectToAction("Index", "TodoList");
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, TodoTaskEditViewModel model)
    {
        if (id != model.Id)
        {
            return BadRequest();
        }

        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId))
        {
            return RedirectToAction("Login", "Account");
        }

        try
        {
            var dto = new TodoTaskDto
            {
                Id = model.Id,
                Title = model.Title,
                Description = model.Description,
                DueDate = model.DueDate ?? DateTime.MinValue,
                Priority = (Entities.Enums.TaskPriority)model.Priority,  // Явное преобразование int → enum
                TodoListId = model.TodoListId
            };

            await _todoTaskService.UpdateAsync(dto, userId);

            TempData["SuccessMessage"] = "Task updated successfully.";
            return RedirectToAction("Details", new { id = model.Id });
        }
        catch (ApiConnectionException)
        {
            ModelState.AddModelError("", "Unable to connect to the server. Please try again later.");
            return View(model);
        }
        catch (UnauthorizedAccessException)
        {
            TempData["ErrorMessage"] = "You don't have permission to edit this task.";
            return RedirectToAction("Index", "TodoList");
        }
        catch (KeyNotFoundException)
        {
            TempData["ErrorMessage"] = "Task not found.";
            return RedirectToAction("Index", "TodoList");
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id, int todoListId)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId))
        {
            return RedirectToAction("Login", "Account");
        }

        try
        {
            await _todoTaskService.DeleteAsync(id, userId);

            TempData["SuccessMessage"] = "Task deleted successfully.";
            return RedirectToAction("Details", "TodoList", new { id = todoListId });
        }
        catch (ApiConnectionException)
        {
            TempData["ErrorMessage"] = "Unable to connect to the server. Please try again later.";
        }
        catch (UnauthorizedAccessException)
        {
            TempData["ErrorMessage"] = "You don't have permission to delete this task.";
        }
        catch (KeyNotFoundException)
        {
            TempData["ErrorMessage"] = "Task not found.";
        }

        return RedirectToAction("Details", "TodoList", new { id = todoListId });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ToggleComplete(int id)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized();
        }

        try
        {
            var updatedTask = await _todoTaskService.ToggleCompletedAsync(id, userId);
            return Json(new { success = true, isCompleted = updatedTask.IsCompleted });
        }
        catch (UnauthorizedAccessException)
        {
            TempData["ErrorMessage"] = "You don't have permission to delete this todo list.";
            return RedirectToAction(nameof(Index));
        }
        catch (KeyNotFoundException)
        {
            TempData["ErrorMessage"] = "Todo list not found.";
            return RedirectToAction(nameof(Index));
        }
        catch (ApiConnectionException)
        {
            TempData["ErrorMessage"] = "Unable to connect to the server. Please try again later.";
            return RedirectToAction(nameof(Index));
        }
        catch (ApiResponseException)
        {
            TempData["ErrorMessage"] = "Invalid response from the server. Please try again.";
            return RedirectToAction(nameof(Index));
        }
    }
}
