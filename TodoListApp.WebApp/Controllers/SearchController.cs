using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TodoListApp.Models.ViewModels;
using TodoListApp.Services.Interfaces;

namespace TodoListApp.WebApp.Controllers;

[Authorize]
public class SearchController : Controller
{
    private readonly ISearchService _searchService;
    private readonly ILogger<SearchController> _logger;

    public SearchController(ISearchService searchService, ILogger<SearchController> logger)
    {
        _searchService = searchService;
        _logger = logger;
    }

    [HttpGet]
    public IActionResult Index()
    {
        return View(new SearchViewModel());
    }

    [HttpGet]
    public async Task<IActionResult> Results(
        string q,
        DateTime? startDate = null,
        DateTime? endDate = null,
        int pageNumber = 1)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (string.IsNullOrEmpty(userId))
        {
            return RedirectToAction("Login", "Account");
        }

        try
        {
            var pagedResult = await _searchService.SearchTasksAsync(
                userId, q, startDate, endDate, pageNumber, 10);

            var viewModel = new SearchResultsViewModel
            {
                SearchQuery = q,
                StartDate = startDate,
                EndDate = endDate,
                Tasks = pagedResult.Items.Select(t => new SearchTaskViewModel
                {
                    Id = t.Id,
                    Title = t.Title,
                    Description = t.Description,
                    Status = t.Status,
                    Priority = t.Priority,
                    DueDate = t.DueDate,
                    CreatedDate = t.CreatedDate,
                    IsCompleted = t.IsCompleted,
                    IsOverdue = t.DueDate.HasValue && t.DueDate.Value.Date < DateTime.UtcNow.Date && !t.IsCompleted,
                    TodoListId = t.TodoListId
                }).ToList(),
                PageNumber = pagedResult.PageNumber,
                TotalPages = pagedResult.TotalPages,
                TotalCount = pagedResult.TotalCount
            };

            return View(viewModel);
        }
        catch (ArgumentException ex)
        {
            _logger.LogError(ex, "Error searching tasks");
            TempData["ErrorMessage"] = "An error occurred while searching";
            return View(new SearchResultsViewModel { SearchQuery = q });
        }
    }
}
