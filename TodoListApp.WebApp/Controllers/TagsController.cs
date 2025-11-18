using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TodoListApp.Models.ViewModels;
using TodoListApp.Services.Exceptions;
using TodoListApp.Services.Interfaces;

namespace TodoListApp.WebApp.Controllers;

[Authorize]
public class TagsController : Controller
{
    private readonly ITagService _tagService;
    public TagsController(ITagService tagService)
    {
        _tagService = tagService;
    }

    public async Task<IActionResult> Index()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (string.IsNullOrEmpty(userId))
        {
            return RedirectToAction("Login", "Account");
        }

        try
        {
            var tags = await _tagService.GetAllTagsAsync(userId);

            var viewModel = tags.Select(t => new TagViewModel
            {
                Id = t.Id,
                Name = t.Name,
                TaskCount = t.TaskCount
            }).ToList();

            return View(viewModel);
        }
        catch (ApiConnectionException)
        {
            TempData["ErrorMessage"] = "Unable to connect to the server.";
            return RedirectToAction("Index", "TodoList");
        }
    }

    public async Task<IActionResult> TasksByTag(int id)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (string.IsNullOrEmpty(userId))
        {
            return RedirectToAction("Login", "Account");
        }

        try
        {
            var tasks = await _tagService.GetTasksByTagIdAsync(id, userId);
            var tags = await _tagService.GetAllTagsAsync(userId);
            var tag = tags.FirstOrDefault(t => t.Id == id);

            var viewModel = new TasksByTagViewModel
            {
                TagId = id,
                TagName = tag?.Name ?? "Unknown",
                Tasks = tasks.Select(t => new TaggedTaskViewModel
                {
                    Id = t.Id,
                    Title = t.Title,
                    Description = t.Description,
                    IsCompleted = t.IsCompleted,
                    DueDate = t.DueDate,
                    TodoListId = t.TodoListId,
                    IsOverdue = t.DueDate.HasValue && t.DueDate.Value.Date < DateTime.UtcNow.Date && !t.IsCompleted
                }).ToList()
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
    public async Task<IActionResult> AddTagToTask(int taskId, string tagName)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized();
        }

        try
        {
            await _tagService.AddTagToTaskAsync(taskId, tagName, userId);
            return Json(new { success = true });
        }
        catch (ApiConnectionException)
        {
            TempData["ErrorMessage"] = "Unable to connect to the server.";
            return RedirectToAction("Index", "TodoList");
        }
    }

    [HttpPost]
    public async Task<IActionResult> RemoveTagFromTask(int taskId, int tagId)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized();
        }

        try
        {
            await _tagService.RemoveTagFromTaskAsync(taskId, tagId, userId);
            return Json(new { success = true });
        }
        catch (ApiConnectionException)
        {
            TempData["ErrorMessage"] = "Unable to connect to the server.";
            return RedirectToAction("Index", "TodoList");
        }
    }
}
