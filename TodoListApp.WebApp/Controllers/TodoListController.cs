using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TodoListApp.Models.Common;
using TodoListApp.Models.ViewModels;
using TodoListApp.Services.Exceptions;
using TodoListApp.Services.Interfaces;

namespace TodoListApp.WebApp.Controllers;

[Authorize]
[Serializable]
public class TodoListController : Controller
{
    private readonly ITodoListService _todoListService;
    private readonly ITodoTaskService _todoTaskService;

    public TodoListController(ITodoListService todoListService, ITodoTaskService todoTaskService)
    {
        _todoListService = todoListService;
        _todoTaskService = todoTaskService;
    }
    [HttpGet]
    public async Task<IActionResult> Index(int pageNumber = 1)
    {
        const int pageSize = 10;
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (string.IsNullOrEmpty(userId))
        {
            return RedirectToAction("Login", "Account");
        }

        try
        {
            var pagedResult = await _todoListService.GetUserTodoListsAsync(userId, pageNumber, pageSize);

            var viewModelResult = new PagedResult<TodoListViewModel>
            {
                Items = pagedResult.Items.Select(dto => new TodoListViewModel
                {
                    Id = dto.Id,
                    Title = dto.Title,
                    Description = dto.Description,
                    CreatedDate = dto.CreatedDate,
                    TaskCount = dto.TaskCount,
                }).ToList(),
                TotalCount = pagedResult.TotalCount,
                PageNumber = pagedResult.PageNumber,
                PageSize = pagedResult.PageSize
            };

            return View(viewModelResult);
        }
        catch (ApiConnectionException)
        {
            TempData["ErrorMessage"] = "Unable to connect to the server. Please try again later.";
            return View(CreateEmptyPagedResult(pageNumber, pageSize));
        }
        catch (ApiResponseException)
        {
            TempData["ErrorMessage"] = "Received invalid response from the server. Please try again.";
            return View(CreateEmptyPagedResult(pageNumber, pageSize));
        }
        catch (UnauthorizedAccessException)
        {
            TempData["ErrorMessage"] = "You don't have permission to access this resource.";
            return RedirectToAction("AccessDenied", "Account");
        }
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
            var listDto = await _todoListService.GetTodoListByIdAsync(id, userId);

            if (listDto == null)
            {
                TempData["ErrorMessage"] = "Todo list not found.";
                return RedirectToAction(nameof(Index));
            }

            if (listDto.OwnerId != userId)
            {
                TempData["ErrorMessage"] = "You don't have permission to view this todo list.";
                return RedirectToAction(nameof(Index));
            }

            var taskDto = await _todoListService.GetTasksByIdAsync(id, userId);

            var viewModel = new TodoListDetailsViewModel
            {
                Id = listDto.Id,
                Title = listDto.Title,
                Description = listDto.Description,
                CreatedDate = listDto.CreatedDate,
                Tasks = taskDto.Select(t => new TodoTaskViewModel
                {
                    Id = t.Id,
                    Title = t.Title,
                    Description = t.Description,
                    IsCompleted = t.IsCompleted,
                    DueDate = t.DueDate,
                    CreatedDate = t.CreatedDate,
                })
                .OrderBy(t => t.IsCompleted)
                .ThenBy(t => t.CreatedDate)
                .ToList(),
            };

            viewModel.TotalTasksCount = viewModel.Tasks.Count;
            viewModel.CompletedTasksCount = viewModel.Tasks.Count(t => t.IsCompleted);

            return View(viewModel);
        }
        catch (ApiConnectionException)
        {
            TempData["ErrorMessage"] = "Unable to connect to the server. Please try again later.";
            return RedirectToAction(nameof(Index));
        }
        catch (ApiResponseException)
        {
            TempData["ErrorMessage"] = "Received invalid response from the server. Please try again.";
            return RedirectToAction(nameof(Index));
        }
        catch (KeyNotFoundException)
        {
            TempData["ErrorMessage"] = "Todo list not found.";
            return RedirectToAction(nameof(Index));
        }
    }

    [HttpGet]
    public IActionResult Create()
    {
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken()]
    public async Task<IActionResult> Create(TodoListViewModel model)
    {
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
            var dto = new TodoListApp.Models.DTOs.TodoListDto
            {
                Title = model.Title,
                Description = model.Description,
            };
            await _todoListService.CreateAsync(dto, userId);
            TempData["SuccessMessage"] = "Todo list created successfully.";
            return RedirectToAction(nameof(Index));
        }
        catch (ArgumentException ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
            return View(model);
        }
        catch (ApiConnectionException)
        {
            ModelState.AddModelError("", "Unable to connect to the server. Please try again later.");
            return View(model);
        }
        catch (ApiResponseException)
        {
            ModelState.AddModelError("", "Received invalid response from the server. Please try again.");
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
            var dto = await _todoListService.GetTodoListByIdAsync(id, userId);

            if (dto == null)
            {
                TempData["ErrorMessage"] = "Todo list not found.";
                return RedirectToAction(nameof(Index));
            }

            var viewModel = new TodoListEditViewModel
            {
                Id = dto.Id,
                Title = dto.Title,
                Description = dto.Description,
            };

            return View(viewModel);
        }
        catch (ApiConnectionException)
        {
            TempData["ErrorMessage"] = "Unable to connect to the server.";
            return RedirectToAction(nameof(Index));
        }
        catch (ApiResponseException)
        {
            TempData["ErrorMessage"] = "Invalid response from the server.";
            return RedirectToAction(nameof(Index));
        }
        catch (KeyNotFoundException)
        {
            TempData["ErrorMessage"] = "Todo list not found.";
            return RedirectToAction(nameof(Index));
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, TodoListEditViewModel model)
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
            var dto = new TodoListApp.Models.DTOs.TodoListDto
            {
                Id = model.Id,
                Title = model.Title,
                Description = model.Description
            };

            await _todoListService.UpdateAsync(dto, userId);

            TempData["SuccessMessage"] = "Todo list updated successfully!";
            return RedirectToAction(nameof(Index));
        }
        catch (UnauthorizedAccessException)
        {
            TempData["ErrorMessage"] = "You don't have permission to edit this todo list.";
            return RedirectToAction(nameof(Index));
        }
        catch (ApiConnectionException)
        {
            ModelState.AddModelError("", "Unable to connect to the server. Please try again later.");
            return View(model);
        }
        catch (ApiResponseException)
        {
            ModelState.AddModelError("", "Invalid response from the server. Please try again.");
            return View(model);
        }
        catch (KeyNotFoundException)
        {
            TempData["ErrorMessage"] = "Todo list not found.";
            return RedirectToAction(nameof(Index));
        }
    }

    [HttpGet]
    public async Task<IActionResult> Delete(int id)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (string.IsNullOrEmpty(userId))
        {
            return RedirectToAction("Login", "Account");
        }

        try
        {
            var dto = await _todoListService.GetTodoListByIdAsync(id, userId);

            if (dto == null)
            {
                TempData["ErrorMessage"] = "Todo list not found.";
                return RedirectToAction(nameof(Index));
            }

            var viewModel = new TodoListViewModel
            {
                Id = dto.Id,
                Title = dto.Title,
                Description = dto.Description,
                CreatedDate = dto.CreatedDate,
                TaskCount = dto.TaskCount,
            };

            return View(viewModel);
        }
        catch (ApiConnectionException)
        {
            TempData["ErrorMessage"] = "Unable to connect to the server.";
            return RedirectToAction(nameof(Index));
        }
        catch (ApiResponseException)
        {
            TempData["ErrorMessage"] = "Invalid response from the server.";
            return RedirectToAction(nameof(Index));
        }
        catch (KeyNotFoundException)
        {
            TempData["ErrorMessage"] = "Todo list not found.";
            return RedirectToAction(nameof(Index));
        }
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (string.IsNullOrEmpty(userId))
        {
            return RedirectToAction("Login", "Account");
        }

        try
        {
            await _todoListService.DeleteAsync(id, userId);

            TempData["SuccessMessage"] = "Todo list deleted successfully!";
            return RedirectToAction(nameof(Index));
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

    private PagedResult<TodoListViewModel> CreateEmptyPagedResult(int pageNumber, int pageSize)
    {
        return new PagedResult<TodoListViewModel>
        {
            Items = new List<TodoListViewModel>(),
            TotalCount = 0,
            PageNumber = pageNumber,
            PageSize = pageSize
        };
    }
}

