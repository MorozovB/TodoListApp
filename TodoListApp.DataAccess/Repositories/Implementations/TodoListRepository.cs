using TodoListApp.DataAccess.Repositories.Interfaces;
using TodoListApp.DataAccess.Context;
using TodoListApp.Entities;
using Microsoft.EntityFrameworkCore;

namespace TodoListApp.DataAccess.Repositories.Implementations;
public class TodoListRepository : ITodoListRepository
{
    private readonly TodoListDbContext _dbContext;

    public TodoListRepository(TodoListDbContext dbContext)
    {
        this._dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
    }

    public async Task<IEnumerable<TodoListEntity>> GetAllByOwnerIdAsync(string ownerId)
    {
        return await this._dbContext.TodoLists
            .Where(tl => tl.OwnerId == ownerId)
            .Include(tl => tl.Tasks)
            .OrderByDescending(tl => tl.CreatedDate)
            .ToListAsync();
    }

    public async Task<TodoListEntity?> GetByIdAsync(int id)
    {
        return await this._dbContext.TodoLists
            .Include(tl => tl.Tasks)
            .FirstOrDefaultAsync(tl => tl.Id == id);
    }

    public async Task<TodoListEntity> CreateAsync(TodoListEntity todoList)
    {
        ArgumentNullException.ThrowIfNull(todoList);
        var entityEntry = await this._dbContext.TodoLists.AddAsync(todoList);
        _ = await this._dbContext.SaveChangesAsync();
        return entityEntry.Entity;
    }

    public async Task UpdateSync(TodoListEntity todoList)
    {
        ArgumentNullException.ThrowIfNull(todoList);
        _ = this._dbContext.TodoLists.Update(todoList);
        _ = await this._dbContext.SaveChangesAsync();
    }

    public async Task DeleteAsync(int id)
    {
        var todoLIst = await this.GetByIdAsync(id);

        if (todoLIst is null)
        {
            throw new InvalidOperationException($"Todo list with id {id} not found.");
        }

        _ = this._dbContext.TodoLists.Remove(todoLIst);
        _ = await this._dbContext.SaveChangesAsync();
    }

    public async Task<bool> ExistsAsync(int id)
    {
        return await this._dbContext.TodoLists.AnyAsync(tl => tl.Id == id);
    }

    public async Task<bool> IsOwnerAsync(int todoListId, string userId)
    {
        var todoList = await this.GetByIdAsync(todoListId);
        if (todoList is null)
        {
            return false;
        }
        return this._dbContext.TodoLists.AnyAsync(tl => tl.Id == todoListId && tl.OwnerId == userId).Result;
    }
}
