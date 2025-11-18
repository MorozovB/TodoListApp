using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TodoListApp.Models.ViewModels;
using TodoListApp.Services.Exceptions;
using TodoListApp.Services.Interfaces;

namespace TodoListApp.WebApp.Controllers;

[Authorize]
public class AssignedTasksController : Controller
{
    private readonly IAssignedTasksService _assignedTasksService;
    private readonly ILogger<AssignedTasksController> _logger;

    public AssignedTasksController(IAssignedTasksService assignedTasksService, ILogger<AssignedTasksController> logger)
    {
        _assignedTasksService = assignedTasksService;
        _logger = logger;
    }

    [HttpGet]
    public async Task<IActionResult> Index(
        Entities.Enums.StatusOfTask? status = null,
        string sortBy = "duedate",
        int pageNumber = 1)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (string.IsNullOrEmpty(userId))
        {
            return RedirectToAction("Login", "Account");
        }

        try
        {
            var pagedResult = await _assignedTasksService.GetAssignedTasksAsync(
                userId, status, sortBy, pageNumber, 10);

            var viewModel = new AssignedTasksViewModel
            {
                Tasks = pagedResult.Items.Select(t => new AssignedTaskViewModel
                {
                    Id = t.Id,
                    Title = t.Title,
                    Description = t.Description,
                    Status = t.Status,
                    Priority = t.Priority,
                    DueDate = t.DueDate,
                    IsOverdue = t.DueDate.HasValue && t.DueDate.Value.Date < DateTime.UtcNow.Date && t.Status != Entities.Enums.StatusOfTask.Completed,
                    TodoListId = t.TodoListId
                }).ToList(),
                CurrentFilter = status,
                CurrentSort = sortBy,
                PageNumber = pagedResult.PageNumber,
                TotalPages = pagedResult.TotalPages,
                TotalCount = pagedResult.TotalCount
            };

            return View(viewModel);
        }
        catch (ApiConnectionException ex)
        {
            _logger.LogError(ex, "Unable to connect to the server");
            TempData["ErrorMessage"] = "Unable to load assigned tasks due to server connection issues.";
            return View(new AssignedTasksViewModel());
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogError(ex, "Invalid operation while loading tasks");
            TempData["ErrorMessage"] = "Error loading tasks.";
            return View(new AssignedTasksViewModel());
        }
    }

    [HttpPost]
    public async Task<IActionResult> ChangeStatus(int taskId, Entities.Enums.StatusOfTask newStatus)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized();
        }

        try
        {
            await _assignedTasksService.ChangeStatusOfTaskAsync(taskId, newStatus, userId);
            TempData["SuccessMessage"] = "Task status updated successfully";
            return RedirectToAction(nameof(Index));
        }
        catch (ApiConnectionException ex)
        {
            _logger.LogError(ex, "Unable to connect to the server");
            TempData["ErrorMessage"] = "Unable to load assigned tasks due to server connection issues.";
            return View(new AssignedTasksViewModel());
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogError(ex, "Invalid operation while loading tasks");
            TempData["ErrorMessage"] = "Error loading tasks.";
            return View(new AssignedTasksViewModel());
        }
    }
}